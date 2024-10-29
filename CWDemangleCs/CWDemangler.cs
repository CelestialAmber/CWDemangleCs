using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Xml.Linq;


namespace CWDemangleCs
{
    /// Options for [demangle].
    public struct DemangleOptions
    {
        /// Replace `(void)` function parameters with `()`
        public bool omit_empty_parameters;
        /// Enable Metrowerks extension types (`__int128`, `__vec2x32float__`, etc.)
        ///
        /// Disabled by default since they conflict with template argument literals
        /// and can't always be demangled correctly.
        public bool mw_extensions;

        public DemangleOptions()
        {
            omit_empty_parameters = true;
            mw_extensions = false;
        }

        public DemangleOptions(bool omit_empty_parameters, bool mw_extensions)
        {
            this.omit_empty_parameters = omit_empty_parameters;
            this.mw_extensions = mw_extensions;
        }
    }

    public class CWDemangler
    {
        static (string, string, string) parse_qualifiers(string str) {
            string pre = "";
            string post = "";
            foreach(char c in str) {
                bool foundNonQualifier = false;

                switch(c) {
                    case 'P':
                        if(pre == ""){
                            post = post.Insert(0, "*");
                        }else{
                            post = post.Insert(0, string.Format("* {0}", pre.TrimEnd()));
                            pre = "";
                        }
                        break;
                    case 'R':
                        if(pre == ""){
                            post = post.Insert(0, "&");
                        }else{
                            post = post.Insert(0, string.Format("& {0}", pre.TrimEnd()));
                            pre = "";
                        }
                        break;
                    case 'C':
                        pre += "const ";
                        break;
                    case 'V':
                        pre += "volatile ";
                        break;
                    case 'U':
                        pre += "unsigned ";
                        break;
                    case 'S':
                        pre += "signed ";
                        break;
                    default:
                        foundNonQualifier = true;
                        break;
                };

                if (foundNonQualifier) break;
                str = str.Substring(1);
            }
            post = post.TrimEnd();
            return (pre, post, str);
        }
        
        static (int, string)? parse_digits(string str) {
            bool containsNonDigit = false;
            int idx = 0;
            while(idx < str.Length)
            {
                if (!Char.IsAsciiDigit(str[idx]))
                {
                    containsNonDigit = true;
                    break;
                }
                idx++;
            }

            int val;

            if (containsNonDigit){
                if (!int.TryParse(str.Substring(0, idx), out val)) return null;
                return (val, str.Substring(idx));
            } else
            {
                // all digits!
                if (!int.TryParse(str, out val)) return null;
                return (val, "");
            }
        }

        public static (string, string)? demangle_template_args(string str, DemangleOptions options) {
            int start_idx = str.IndexOf('<');
            string tmpl_args;
            
            if(start_idx != -1){
                int end_idx = str.LastIndexOf('>');
                if(end_idx == -1 || end_idx < start_idx){
                    return null;
                }

                string args = str.Substring(start_idx + 1, end_idx - (start_idx + 1));
                str = str.Substring(0, start_idx); //might be inclusive of the char at start_idx??
                tmpl_args = "<";

                while(args != ""){
                    (string, string, string)? result = demangle_arg(args, options);
                    if(!result.HasValue) return null;
                    (string arg, string arg_post, string rest) = result.Value;
                    tmpl_args += arg;
                    tmpl_args += arg_post;

                    if(rest == ""){
                        break;
                    }else{
                        tmpl_args += ", ";
                    }

                    args = rest.Substring(1);
                }

                tmpl_args += ">";
            } else {
                tmpl_args = "";
            };
            return (str, tmpl_args);
        }
        
        public static (string, string, string)? demangle_name(string str, DemangleOptions options) {
            (int, string)? result = parse_digits(str);
            if (!result.HasValue) return null;
            (int size, string rest) = result.Value;

            if (rest.Length < size){
                return null;
            }

            (string, string)? result1 = demangle_template_args(rest.Substring(0, size), options);
            if (!result1.HasValue) return null;
            (string name, string args) = result1.Value;
            return (name, string.Format("{0}{1}",name,args), rest.Substring(size));
        }
        
        public static (string, string, string)? demangle_qualified_name(string str, DemangleOptions options){
            if(str.StartsWith('Q')){
                if(str.Length < 3){
                    return null;
                }

                int count;

                if (!int.TryParse(str.Substring(1, 1), out count)) return null;
                str = str.Substring(2);

                string last_class = "";
                string qualified = "";

                for(int i = 0; i < count; i++) {
                    (string, string, string)? result = demangle_name(str, options);
                    if (!result.HasValue) return null;
                    (string class_name, string full, string rest) = result.Value;
                    qualified += full;
                    last_class = class_name;
                    str = rest;
                    if(i < count - 1){
                        qualified += "::";
                    }
                }
                return (last_class, qualified, str);
            }
            else{
                return demangle_name(str, options);
            }
        }
        
        public static (string, string, string)? demangle_arg(string str, DemangleOptions options){
            // Negative constant
            if(str.StartsWith('-')){
                (int, string)? parseResult = parse_digits(str.Substring(1));
                if (!parseResult.HasValue) return null;
                (int size, string restVal) = parseResult.Value;
                string outVal = $"-{size}";
                return (outVal, "", restVal);
            }

            string result = "";
            (string pre, string post, string rest) = parse_qualifiers(str);
            result += pre;
            str = rest;

            // Disambiguate arguments starting with a number
            if (str.Length > 0 && Char.IsAsciiDigit(str[0])){
                (int, string)? parseResult = parse_digits(str);
                if (!parseResult.HasValue) return null;
                (int num, rest) = parseResult.Value;
                // If the number is followed by a comma or the end of the string, it's a template argument
                if(rest == "" || rest.StartsWith(',')){
                    // ...or a Metrowerks extension type
                    if(options.mw_extensions){
                        string t =
                              num == 1 ? "__int128"
                            : num == 2 ? "__vec2x32float__"
                            : null;

                        if(t != null){
                            result += t;
                            return (result, post, rest);
                        }
                    }
                    result += $"{num}";
                    result += post;
                    return (result, "", rest);
                }
                // Otherwise, it's (probably) the size of a type
                (string, string, string)? demangleNameResult = demangle_name(str, options);
                if (!demangleNameResult.HasValue) return null;
                (_, string qualified, rest) = demangleNameResult.Value;
                result += qualified;
                result += post;
                return (result, "", rest);
            }

            // Handle qualified names
            if(str.StartsWith('Q')){
                (string, string, string)? demangleQualResult = demangle_qualified_name(str, options);
                if (!demangleQualResult.HasValue) return null;
                (_, string qualified, rest) = demangleQualResult.Value;
                result += qualified;
                result += post;
                return (result, "", rest);
            }

            bool is_member = false;
            bool const_member = false;
            if(str.StartsWith('M')){
                is_member = true;
                (string, string, string)? demangleQualResult = demangle_qualified_name(str.Substring(1), options);
                if (!demangleQualResult.HasValue) return null;
                (_, string member, rest) = demangleQualResult.Value;
                pre = $"{member}::*{pre}";
                if(!rest.StartsWith('F')){
                    return null;
                }
                str = rest;
            }
            if(is_member || str.StartsWith('F')){
                str = str.Substring(1);
                if(is_member){
                    // "const void*, const void*" or "const void*, void*"
                    if(str.StartsWith("PCvPCv")){
                        const_member = true;
                        str = str.Substring(6);
                    }
                    else if(str.StartsWith("PCvPv")){
                        str = str.Substring(5);
                    }
                    else
                    {
                        return null;
                    }
                }
                else if(post.StartsWith('*')){
                    post = post.Substring(1).TrimStart();
                    pre = $"*{pre}";
                }
                else
                {
                    return null;
                }

                (string, string)? demangleFuncArgsResult = demangle_function_args(str, options);
                if (!demangleFuncArgsResult.HasValue) return null;
                (string args, rest) = demangleFuncArgsResult.Value;

                if(!rest.StartsWith('_')){
                    return null;
                }

                (string, string, string)? demangleArgResult = demangle_arg(rest.Substring(1), options);
                if (!demangleArgResult.HasValue) return null;

                (string ret_pre, string ret_post, rest) =  demangleArgResult.Value;
                string const_str = const_member ? " const" : "";
                string res_pre = $"{ret_pre} ({pre}{post}";
                string res_post = $")({args}){const_str}{ret_post}";
                return (res_pre, res_post, rest);
            }

            if(str.StartsWith('A')){
                (int, string)? parseResult = parse_digits(str.Substring(1));
                if (!parseResult.HasValue) return null;
                (int count, rest) = parseResult.Value;

                if(!rest.StartsWith('_')){
                    return null;
                }

                (string, string, string)? demangleArgResult = demangle_arg(rest.Substring(1), options);
                if (!demangleArgResult.HasValue) return null;
                (string arg_pre, string arg_post, rest) = demangleArgResult.Value;

                if(post != ""){
                    post = $"({post})";
                }
                result = $"{pre}{arg_pre}{post}";
                string ret_post = $"[{count}]{arg_post}";
                return (result, ret_post, rest);
            }

            if (str.Length == 0) return null;
            
            string type = "";

            switch (str[0])
            {
                case 'i':
                    type = "int";
                    break;
                case 'b':
                    type = "bool";
                    break;
                case 'c':
                    type = "char";
                    break;
                case 's':
                    type = "short";
                    break;
                case 'l':
                    type = "long";
                    break;
                case 'x':
                    type = "long long";
                    break;
                case 'f':
                    type = "float";
                    break;
                case 'd':
                    type = "double";
                    break;
                case 'w':
                    type = "wchar_t";
                    break;
                case 'v':
                    type = "void";
                    break;
                case 'e':
                    type = "...";
                    break;
                case '1':
                    if (options.mw_extensions) type = "__int128";
                    break;
                case '2':
                    if (options.mw_extensions) type = "__vec2x32float__";
                    break;
                case '_': return (result, "", rest);
                default: return null;
            }

            result += type;

            result += post;
            return (result, "", str.Substring(1));
        }
        
        public static (string, string)? demangle_function_args(string str, DemangleOptions options){
            string result = "";

            while(str != ""){
                if(result != ""){
                    result += ", ";
                }

                (string, string, string)? demangleArgResult = demangle_arg(str, options);
                if (!demangleArgResult.HasValue) return null;
                (string arg, string arg_post, string rest) = demangleArgResult.Value;

                result += arg;
                result += arg_post;
                str = rest;

                if(str.StartsWith('_') || str.StartsWith(',')){
                    break;
                }
            }

            return (result, str);
        }
        
        static string demangle_special_function(string str, string class_name, DemangleOptions options) {
            if(str.StartsWith("op")){
                string rest = str.Substring(2);
                (string, string, string)? demangleArgResult = demangle_arg(rest, options);
                if (!demangleArgResult.HasValue) return null;
                (string arg_pre, string arg_post, string _) = demangleArgResult.Value;
                return $"operator {arg_pre}{arg_post}";
            }

            (string, string)? result = demangle_template_args(str, options);
            if (!result.HasValue) return null;
            (string op, string args) = result.Value;

            string funcName;

            switch(op){
                case "dt": return $"~{class_name}{args}";
                case "ct":
                    funcName = class_name;
                    break;
                case "nw":
                    funcName = "operator new";
                    break;
                case "nwa":
                    funcName = "operator new[]";
                    break;
                case "dl":
                    funcName = "operator delete";
                    break;
                case "dla":
                    funcName = "operator delete[]";
                    break;
                case "pl":
                    funcName = "operator+";
                    break;
                case "mi":
                    funcName = "operator-";
                    break;
                case "ml":
                    funcName = "operator*";
                    break;
                case "dv":
                    funcName = "operator/";
                    break;
                case "md":
                    funcName = "operator%";
                    break;
                case "er":
                    funcName = "operator^";
                    break;
                case "ad":
                    funcName = "operator&";
                    break;
                case "or":
                    funcName = "operator|";
                    break;
                case "co":
                    funcName = "operator~";
                    break;
                case "nt":
                    funcName = "operator!";
                    break;
                case "as":
                    funcName = "operator=";
                    break;
                case "lt":
                    funcName = "operator<";
                    break;
                case "gt":
                    funcName = "operator>";
                    break;
                case "apl":
                    funcName = "operator+=";
                    break;
                case "ami":
                    funcName = "operator-=";
                    break;
                case "amu":
                    funcName = "operator*=";
                    break;
                case "adv":
                    funcName = "operator/=";
                    break;
                case "amd":
                    funcName = "operator%=";
                    break;
                case "aer":
                    funcName = "operator^=";
                    break;
                case "aad":
                    funcName = "operator&=";
                    break;
                case "aor":
                    funcName = "operator|=";
                    break;
                case "ls":
                    funcName = "operator<<";
                    break;
                case "rs":
                    funcName = "operator>>";
                    break;
                case "ars":
                    funcName = "operator>>=";
                    break;
                case "als":
                    funcName = "operator<<=";
                    break;
                case "eq":
                    funcName = "operator==";
                    break;
                case "ne":
                    funcName = "operator!=";
                    break;
                case "le":
                    funcName = "operator<=";
                    break;
                case "ge":
                    funcName = "operator>=";
                    break;
                case "aa":
                    funcName = "operator&&";
                    break;
                case "oo":
                    funcName = "operator||";
                    break;
                case "pp":
                    funcName = "operator++";
                    break;
                case "mm":
                    funcName = "operator--";
                    break;
                case "cm":
                    funcName = "operator,";
                    break;
                case "rm":
                    funcName = "operator->*";
                    break;
                case "rf":
                    funcName = "operator->";
                    break;
                case "cl":
                    funcName = "operator()";
                    break;
                case "vc":
                    funcName = "operator[]";
                    break;
                case "vt":
                    funcName = "__vtable";
                    break;
                default: return $"__{op}{args}";
            }

            return string.Format("{0}{1}", funcName, args);
        }

        static bool IsAscii(string s)
        {
            return System.Text.Encoding.UTF8.GetByteCount(s) == s.Length;
        }
        
        /// Demangle a symbol name.
        ///
        /// Returns `null` if the input is not a valid mangled name.
        public static string demangle(string str, DemangleOptions options) {
            if(!IsAscii(str)){
                return null;
            }
        
            bool special = false;
            bool cnst = false;
            string fn_name;
            string return_type_pre = "";
            string return_type_post = "";
            string qualified = "";
            string static_var = "";
        
            // Handle new static function variables (Wii CW)
            bool guard = str.StartsWith("@GUARD@");
            if(guard || str.StartsWith("@LOCAL@")){
                str = str.Substring(7);
                int idx = str.LastIndexOf('@');
                if (idx == -1) return null;

                string rest = str.Substring(0, idx);
                string var = str.Substring(idx);

                if(guard){
                    static_var = string.Format("{0} guard", var.Substring(1));
                }else{
                    static_var = var.Substring(1);
                }
                str = rest;
            }
        
            if(str.StartsWith("__")){
                special = true;
                str = str.Substring(2);
            }
            {
                int? idxTemp = find_split(str, special, options);
                if (!idxTemp.HasValue) return null;
                int idx = idxTemp.Value;
                // Handle any trailing underscores in the function name
                while(str[idx + 2] == '_'){
                    idx++;
                }

                string fn_name_out = str.Substring(0, idx);
                string rest = str.Substring(idx);

                if(special){
                    if(fn_name_out == "init"){
                        // Special case for double __
                        int rest_idx = rest.Substring(2).IndexOf("__");
                        if (rest_idx == -1) return null;
                        fn_name = str.Substring(0, rest_idx + 6);
                        rest = rest.Substring(rest_idx + 2);
                    }
                    else
                    {
                        fn_name = fn_name_out;
                    }
                }
                else
                {
                    (string, string)? result = demangle_template_args(fn_name_out, options);
                    if (!result.HasValue) return null;
                    (string name, string args) = result.Value;
                    fn_name = $"{name}{args}";
                }

                // Handle old static function variables (GC CW)
                int first_idx = fn_name.IndexOf('$');
                if(first_idx != -1){
                    int second_idx = fn_name.Substring(first_idx + 1).IndexOf('$');
                    if (second_idx == -1) return null;

                    string var = fn_name.Substring(0, first_idx);
                    string restTemp = fn_name.Substring(first_idx);

                    restTemp = restTemp.Substring(1);
                    string var_type = restTemp.Substring(0, second_idx);
                    restTemp = restTemp.Substring(second_idx);

                    if(!var_type.StartsWith("localstatic")){
                        return null;
                    }

                    if(var == "init"){
                        // Sadly, $localstatic doesn't provide the variable name in guard/init
                        static_var = $"{var_type} guard";
                    }
                    else
                    {
                        static_var = var;
                    }

                    fn_name = restTemp.Substring(1);
                }
        
                str = rest.Substring(2);
            }

            string class_name = "";
            if(!str.StartsWith('F')){
                (string, string, string)? result = demangle_qualified_name(str, options);
                if (!result.HasValue) return null;
                (string name, string qualified_name, string rest) = result.Value;
                class_name = name;
                qualified = qualified_name;
                str = rest;
            }
            if(special){
                string result = demangle_special_function(fn_name, class_name, options);
                if (result == null) return null;
                fn_name = result;
            }
            if(str.StartsWith('C')){
                str = str.Substring(1);
                cnst = true;
            }
            if(str.StartsWith('F')){
                str = str.Substring(1);
                (string, string)? result = demangle_function_args(str, options);
                if (!result.HasValue) return null;
                (string args, string rest) = result.Value;
                if(options.omit_empty_parameters && args == "void"){
                    fn_name = $"{fn_name}()";
                }
                else
                {
                    fn_name = $"{fn_name}({args})";
                }
                str = rest;
            }
            if(str.StartsWith('_')){
                str = str.Substring(1);
                (string, string, string)? result = demangle_arg(str, options);
                if (!result.HasValue) return null;
                (string ret_pre, string ret_post, string rest) = result.Value;
                return_type_pre = ret_pre;
                return_type_post = ret_post;
                str = rest;
            }
            if(str != ""){
                return null;
            }
            if(cnst){
                fn_name = $"{fn_name} const";
            }
            if(qualified != ""){
                fn_name = $"{qualified}::{fn_name}";
            }
            if(return_type_pre != ""){
                fn_name = $"{return_type_pre} {fn_name}{return_type_post}";
            }
            if(static_var != ""){
                fn_name = $"{fn_name}::{static_var}";
            }

            return fn_name;
        }
        
        /// Finds the first double underscore in the string, excluding any that are part of a
        /// template argument list or operator name.
        static int? find_split(string s, bool special, DemangleOptions options) {
            int start = 0;

            if(special && s.StartsWith("op")){
                (string, string, string)? result = demangle_arg(s.Substring(2), options);
                if (!result.HasValue) return null;
                (_, _, string rest) =  result.Value;
                start = s.Length - rest.Length;
            }

            int depth = 0;

            for(int i = start; i < s.Length; i++) {
                switch(s[i]){
                    case '<':
                        depth++;
                        break;
                    case '>':
                        depth--;
                        break;
                    case '_':
                        if(s[i + 1] == '_' && depth == 0){
                            return i;
                        }
                        break;
                    default:
                        break;
                }
            }

            return null;
        }
    }
}
