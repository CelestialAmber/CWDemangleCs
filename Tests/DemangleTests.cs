using Microsoft.VisualStudio.TestTools.UnitTesting;
using CWDemangleCs;

namespace Tests
{
    [TestClass]
    public class DemangleTests
    {
        [TestMethod]
        [DataRow("single_ptr<10CModelData>", "single_ptr", "<CModelData>")]
        [DataRow("basic_string<w,Q24rstl14char_traits<w>,Q24rstl17rmemory_allocator>", "basic_string", "<wchar_t, rstl::char_traits<wchar_t>, rstl::rmemory_allocator>")]
        public void TestDemangleTemplateArgs(string mangled, string name, string templateArgs)
        {
            DemangleOptions options = new DemangleOptions();

            (string, string)? result = CWDemangler.demangle_template_args(mangled, options);
            Assert.AreEqual((name, templateArgs), result);
        }

        [TestMethod]
        [DataRow("24single_ptr<10CModelData>", "single_ptr", "single_ptr<CModelData>")]
        [DataRow("66basic_string<w,Q24rstl14char_traits<w>,Q24rstl17rmemory_allocator>", "basic_string", "basic_string<wchar_t, rstl::char_traits<wchar_t>, rstl::rmemory_allocator>")]
        public void TestDemangleName(string mangled, string name, string fullName)
        {
            DemangleOptions options = new DemangleOptions();

            (string, string, string)? result = CWDemangler.demangle_name(mangled, options);
            Assert.AreEqual((name, fullName, ""), result);
        }


        [TestMethod]
        [DataRow("6CActor", "CActor", "CActor")]
        [DataRow("Q29CVector3f4EDim", "EDim", "CVector3f::EDim")]
        [DataRow("Q24rstl66basic_string<w,Q24rstl14char_traits<w>,Q24rstl17rmemory_allocator>", "basic_string", "rstl::basic_string<wchar_t, rstl::char_traits<wchar_t>, rstl::rmemory_allocator>")]
        public void TestDemangleQualifiedName(string mangled, string baseName, string fullName){
            DemangleOptions options = new DemangleOptions();

            (string, string, string)? result = CWDemangler.demangle_qualified_name(mangled, options);
            Assert.AreEqual((baseName, fullName, ""), result);
        }

        [TestMethod]
        [DataRow("v", "void", "", "")]
        [DataRow("b", "bool", "", "")]
        [DataRow("RC9CVector3fUc", "const CVector3f&", "", "Uc")]
        [DataRow("Q24rstl14char_traits<w>,", "rstl::char_traits<wchar_t>", "", ",")]
        [DataRow("PFPCcPCc_v", "void (*", ")(const char*, const char*)", "")]
        [DataRow("RCPCVPCVUi", "const volatile unsigned int* const volatile* const&", "", "")]
        public void TestDemangleArg(string mangled, string typePre, string typePost, string remainder)
        {
            DemangleOptions options = new DemangleOptions();

            (string, string, string)? result = CWDemangler.demangle_arg(mangled, options);
            Assert.AreEqual((typePre, typePost, remainder), result);
        }

        [TestMethod]
        [DataRow("v", "void", "")]
        [DataRow("b", "bool", "")]
        [DataRow("RC9CVector3fUc_x", "const CVector3f&, unsigned char", "_x")]
        public void TestDemangleFunctionArgs(string mangled, string args, string remainder)
        {
            DemangleOptions options = new DemangleOptions();

            (string, string)? result = CWDemangler.demangle_function_args(mangled, options);
            Assert.AreEqual((args, remainder), result);
        }

        [TestMethod]
        [DataRow("__dt__6CActorFv", "CActor::~CActor()")]
        [DataRow("GetSfxHandle__6CActorCFv", "CActor::GetSfxHandle() const")]
        [DataRow("mNull__Q24rstl66basic_string<w,Q24rstl14char_traits<w>,Q24rstl17rmemory_allocator>", "rstl::basic_string<wchar_t, rstl::char_traits<wchar_t>, rstl::rmemory_allocator>::mNull")]
        [DataRow("__ct__Q34rstl495red_black_tree<Ux,Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>,0,Q24rstl215select1st<Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>>,Q24rstl8less<Ux>,Q24rstl17rmemory_allocator>8iteratorFPQ34rstl495red_black_tree<Ux,Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>,0,Q24rstl215select1st<Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>>,Q24rstl8less<Ux>,Q24rstl17rmemory_allocator>4nodePCQ34rstl495red_black_tree<Ux,Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>,0,Q24rstl215select1st<Q24rstl194pair<Ux,Q24rstl175auto_ptr<Q24rstl155map<s,Q24rstl96auto_ptr<Q24rstl77list<Q24rstl35auto_ptr<23CGuiFrameMessageMapNode>,Q24rstl17rmemory_allocator>>,Q24rstl7less<s>,Q24rstl17rmemory_allocator>>>>,Q24rstl8less<Ux>,Q24rstl17rmemory_allocator>6header", "rstl::red_black_tree<unsigned long long, rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>, 0, rstl::select1st<rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>>, rstl::less<unsigned long long>, rstl::rmemory_allocator>::iterator::iterator(rstl::red_black_tree<unsigned long long, rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>, 0, rstl::select1st<rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>>, rstl::less<unsigned long long>, rstl::rmemory_allocator>::node*, const rstl::red_black_tree<unsigned long long, rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>, 0, rstl::select1st<rstl::pair<unsigned long long, rstl::auto_ptr<rstl::map<short, rstl::auto_ptr<rstl::list<rstl::auto_ptr<CGuiFrameMessageMapNode>, rstl::rmemory_allocator>>, rstl::less<short>, rstl::rmemory_allocator>>>>, rstl::less<unsigned long long>, rstl::rmemory_allocator>::header*)")]
        [DataRow("for_each<PP12MultiEmitter,Q23std51binder2nd<Q23std30mem_fun1_t<v,12MultiEmitter,l>,l>>__3stdFPP12MultiEmitterPP12MultiEmitterQ23std51binder2nd<Q23std30mem_fun1_t<v,12MultiEmitter,l>,l>_Q23std51binder2nd<Q23std30mem_fun1_t<v,12MultiEmitter,l>,l>", "std::binder2nd<std::mem_fun1_t<void, MultiEmitter, long>, long> std::for_each<MultiEmitter**, std::binder2nd<std::mem_fun1_t<void, MultiEmitter, long>, long>>(MultiEmitter**, MultiEmitter**, std::binder2nd<std::mem_fun1_t<void, MultiEmitter, long>, long>)")]
        [DataRow("__ct__Q43std3tr16detail383function_imp<PFPCcPCc_v,Q43std3tr16detail334bound_func<v,Q43std3tr16detail59mem_fn_2<v,Q53scn4step7gimmick9shipevent9ShipEvent,PCc,PCc>,Q33std3tr1228tuple<PQ53scn4step7gimmick9shipevent9ShipEvent,Q53std3tr112placeholders6detail5ph<1>,Q53std3tr112placeholders6detail5ph<2>,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat>>,0,1>FRCQ43std3tr16detail383function_imp<PFPCcPCc_v,Q43std3tr16detail334bound_func<v,Q43std3tr16detail59mem_fn_2<v,Q53scn4step7gimmick9shipevent9ShipEvent,PCc,PCc>,Q33std3tr1228tuple<PQ53scn4step7gimmick9shipevent9ShipEvent,Q53std3tr112placeholders6detail5ph<1>,Q53std3tr112placeholders6detail5ph<2>,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat,Q33std3tr13nat>>,0,1>", "std::tr1::detail::function_imp<void (*)(const char*, const char*), std::tr1::detail::bound_func<void, std::tr1::detail::mem_fn_2<void, scn::step::gimmick::shipevent::ShipEvent, const char*, const char*>, std::tr1::tuple<scn::step::gimmick::shipevent::ShipEvent*, std::tr1::placeholders::detail::ph<1>, std::tr1::placeholders::detail::ph<2>, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat>>, 0, 1>::function_imp(const std::tr1::detail::function_imp<void (*)(const char*, const char*), std::tr1::detail::bound_func<void, std::tr1::detail::mem_fn_2<void, scn::step::gimmick::shipevent::ShipEvent, const char*, const char*>, std::tr1::tuple<scn::step::gimmick::shipevent::ShipEvent*, std::tr1::placeholders::detail::ph<1>, std::tr1::placeholders::detail::ph<2>, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat, std::tr1::nat>>, 0, 1>&)")]
        [DataRow("createJointController<11IKJointCtrl>__2MRFP11IKJointCtrlPC9LiveActorUsM11IKJointCtrlFPCvPvPQ29JGeometry64TPosition3<Q29JGeometry38TMatrix34<Q29JGeometry13SMatrix34C<f>>>RC19JointControllerInfo_bM11IKJointCtrlFPCvPvPQ29JGeometry64TPosition3<Q29JGeometry38TMatrix34<Q29JGeometry13SMatrix34C<f>>>RC19JointControllerInfo_b_P15JointController", "JointController* MR::createJointController<IKJointCtrl>(IKJointCtrl*, const LiveActor*, unsigned short, bool (IKJointCtrl::*)(JGeometry::TPosition3<JGeometry::TMatrix34<JGeometry::SMatrix34C<float>>>*, const JointControllerInfo&), bool (IKJointCtrl::*)(JGeometry::TPosition3<JGeometry::TMatrix34<JGeometry::SMatrix34C<float>>>*, const JointControllerInfo&))")]
        [DataRow("execCommand__12JASSeqParserFP8JASTrackM12JASSeqParserFPCvPvP8JASTrackPUl_lUlPUl", "JASSeqParser::execCommand(JASTrack*, long (JASSeqParser::*)(JASTrack*, unsigned long*), unsigned long, unsigned long*)")]
        [DataRow("AddWidgetFnMap__10CGuiWidgetFiM10CGuiWidgetFPCvPvP15CGuiFunctionDefP18CGuiControllerInfo_i", "CGuiWidget::AddWidgetFnMap(int, int (CGuiWidget::*)(CGuiFunctionDef*, CGuiControllerInfo*))")]
        [DataRow("BareFn__FPFPCcPv_v_v", "void BareFn(void (*)(const char*, void*))")]
        [DataRow("BareFn__FPFPCcPv_v_PFPCvPv_v", "void (* BareFn(void (*)(const char*, void*)))(const void*, void*)")]
        [DataRow("SomeFn__FRCPFPFPCvPv_v_RCPFPCvPv_v", "SomeFn(void (*const& (*const&)(void (*)(const void*, void*)))(const void*, void*))")]
        [DataRow("SomeFn__Q29Namespace5ClassCFRCMQ29Namespace5ClassFPCvPCvMQ29Namespace5ClassFPCvPCvPCvPv_v_RCMQ29Namespace5ClassFPCvPCvPCvPv_v", "Namespace::Class::SomeFn(void (Namespace::Class::*const & (Namespace::Class::*const &)(void (Namespace::Class::*)(const void*, void*) const) const)(const void*, void*) const) const")]
        [DataRow("__pl__FRC9CRelAngleRC9CRelAngle", "operator+(const CRelAngle&, const CRelAngle&)")]
        [DataRow("destroy<PUi>__4rstlFPUiPUi", "rstl::destroy<unsigned int*>(unsigned int*, unsigned int*)")]
        [DataRow("__opb__33TFunctor2<CP15CGuiSliderGroup,Cf>CFv", "TFunctor2<CGuiSliderGroup* const, const float>::operator bool() const")]
        [DataRow("__opRC25TToken<15CCharLayoutInfo>__31TLockedToken<15CCharLayoutInfo>CFv", "TLockedToken<CCharLayoutInfo>::operator const TToken<CCharLayoutInfo>&() const")]
        [DataRow("uninitialized_copy<Q24rstl198pointer_iterator<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl89vector<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl17rmemory_allocator>,Q24rstl17rmemory_allocator>,PQ224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo>__4rstlFQ24rstl198pointer_iterator<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl89vector<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl17rmemory_allocator>,Q24rstl17rmemory_allocator>Q24rstl198pointer_iterator<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl89vector<Q224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo,Q24rstl17rmemory_allocator>,Q24rstl17rmemory_allocator>PQ224CSpawnSystemKeyframeData24CSpawnSystemKeyframeInfo", "rstl::uninitialized_copy<rstl::pointer_iterator<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::vector<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::rmemory_allocator>, rstl::rmemory_allocator>, CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo*>(rstl::pointer_iterator<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::vector<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::rmemory_allocator>, rstl::rmemory_allocator>, rstl::pointer_iterator<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::vector<CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo, rstl::rmemory_allocator>, rstl::rmemory_allocator>, CSpawnSystemKeyframeData::CSpawnSystemKeyframeInfo*)")]
        [DataRow("__rf__Q34rstl120list<Q24rstl78pair<i,PFRC10SObjectTagR12CInputStreamRC15CVParamTransfer_C16CFactoryFnReturn>,Q24rstl17rmemory_allocator>14const_iteratorCFv", "rstl::list<rstl::pair<int, const CFactoryFnReturn (*)(const SObjectTag&, CInputStream&, const CVParamTransfer&)>, rstl::rmemory_allocator>::const_iterator::operator->() const")]
        [DataRow("ApplyRipples__FRC14CRippleManagerRA43_A43_Q220CFluidPlaneCPURender13SHFieldSampleRA22_A22_UcRA256_CfRQ220CFluidPlaneCPURender10SPatchInfo", "ApplyRipples(const CRippleManager&, CFluidPlaneCPURender::SHFieldSample(&)[43][43], unsigned char(&)[22][22], const float(&)[256], CFluidPlaneCPURender::SPatchInfo&)")]
        [DataRow("CalculateFluidTextureOffset__14CFluidUVMotionCFfPA2_f", "CFluidUVMotion::CalculateFluidTextureOffset(float, float(*)[2]) const")]
        [DataRow("RenderNormals__FRA43_A43_CQ220CFluidPlaneCPURender13SHFieldSampleRA22_A22_CUcRCQ220CFluidPlaneCPURender10SPatchInfo", "RenderNormals(const CFluidPlaneCPURender::SHFieldSample(&)[43][43], const unsigned char(&)[22][22], const CFluidPlaneCPURender::SPatchInfo&)")]
        [DataRow("Matrix__FfPA2_A3_f", "Matrix(float, float(*)[2][3])")]
        [DataRow("__ct<12CStringTable>__31CObjOwnerDerivedFromIObjUntypedFRCQ24rstl24auto_ptr<12CStringTable>", "CObjOwnerDerivedFromIObjUntyped::CObjOwnerDerivedFromIObjUntyped<CStringTable>(const rstl::auto_ptr<CStringTable>&)")]
        [DataRow("__vt__40TObjOwnerDerivedFromIObj<12CStringTable>", "TObjOwnerDerivedFromIObj<CStringTable>::__vtable")]
        [DataRow("__RTTI__40TObjOwnerDerivedFromIObj<12CStringTable>", "TObjOwnerDerivedFromIObj<CStringTable>::__RTTI")]
        [DataRow("__init__mNull__Q24rstl66basic_string<c,Q24rstl14char_traits<c>,Q24rstl17rmemory_allocator>", "rstl::basic_string<char, rstl::char_traits<char>, rstl::rmemory_allocator>::__init__mNull")]
        [DataRow("__dt__26__partial_array_destructorFv", "__partial_array_destructor::~__partial_array_destructor()")]
        [DataRow("__distance<Q34rstl195red_black_tree<13TGameScriptId,Q24rstl32pair<13TGameScriptId,9TUniqueId>,1,Q24rstl52select1st<Q24rstl32pair<13TGameScriptId,9TUniqueId>>,Q24rstl21less<13TGameScriptId>,Q24rstl17rmemory_allocator>14const_iterator>__4rstlFQ34rstl195red_black_tree<13TGameScriptId,Q24rstl32pair<13TGameScriptId,9TUniqueId>,1,Q24rstl52select1st<Q24rstl32pair<13TGameScriptId,9TUniqueId>>,Q24rstl21less<13TGameScriptId>,Q24rstl17rmemory_allocator>14const_iteratorQ34rstl195red_black_tree<13TGameScriptId,Q24rstl32pair<13TGameScriptId,9TUniqueId>,1,Q24rstl52select1st<Q24rstl32pair<13TGameScriptId,9TUniqueId>>,Q24rstl21less<13TGameScriptId>,Q24rstl17rmemory_allocator>14const_iteratorQ24rstl20forward_iterator_tag", "rstl::__distance<rstl::red_black_tree<TGameScriptId, rstl::pair<TGameScriptId, TUniqueId>, 1, rstl::select1st<rstl::pair<TGameScriptId, TUniqueId>>, rstl::less<TGameScriptId>, rstl::rmemory_allocator>::const_iterator>(rstl::red_black_tree<TGameScriptId, rstl::pair<TGameScriptId, TUniqueId>, 1, rstl::select1st<rstl::pair<TGameScriptId, TUniqueId>>, rstl::less<TGameScriptId>, rstl::rmemory_allocator>::const_iterator, rstl::red_black_tree<TGameScriptId, rstl::pair<TGameScriptId, TUniqueId>, 1, rstl::select1st<rstl::pair<TGameScriptId, TUniqueId>>, rstl::less<TGameScriptId>, rstl::rmemory_allocator>::const_iterator, rstl::forward_iterator_tag)")]
        [DataRow("__ct__Q210Metrowerks683compressed_pair<RQ23std301allocator<Q33std276__tree_deleter<Q23std34pair<Ci,Q212petfurniture8Instance>,Q33std131__multimap_do_transform<i,Q212petfurniture8Instance,Q23std7less<i>,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>,0>13value_compare,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>>4node>,Q210Metrowerks337compressed_pair<Q210Metrowerks12number<Ul,1>,PQ33std276__tree_deleter<Q23std34pair<Ci,Q212petfurniture8Instance>,Q33std131__multimap_do_transform<i,Q212petfurniture8Instance,Q23std7less<i>,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>,0>13value_compare,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>>4node>>FRQ23std301allocator<Q33std276__tree_deleter<Q23std34pair<Ci,Q212petfurniture8Instance>,Q33std131__multimap_do_transform<i,Q212petfurniture8Instance,Q23std7less<i>,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>,0>13value_compare,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>>4node>Q210Metrowerks337compressed_pair<Q210Metrowerks12number<Ul,1>,PQ33std276__tree_deleter<Q23std34pair<Ci,Q212petfurniture8Instance>,Q33std131__multimap_do_transform<i,Q212petfurniture8Instance,Q23std7less<i>,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>,0>13value_compare,Q23std53allocator<Q23std34pair<Ci,Q212petfurniture8Instance>>>4node>", "Metrowerks::compressed_pair<std::allocator<std::__tree_deleter<std::pair<const int, petfurniture::Instance>, std::__multimap_do_transform<int, petfurniture::Instance, std::less<int>, std::allocator<std::pair<const int, petfurniture::Instance>>, 0>::value_compare, std::allocator<std::pair<const int, petfurniture::Instance>>>::node>&, Metrowerks::compressed_pair<Metrowerks::number<unsigned long, 1>, std::__tree_deleter<std::pair<const int, petfurniture::Instance>, std::__multimap_do_transform<int, petfurniture::Instance, std::less<int>, std::allocator<std::pair<const int, petfurniture::Instance>>, 0>::value_compare, std::allocator<std::pair<const int, petfurniture::Instance>>>::node*>>::compressed_pair(std::allocator<std::__tree_deleter<std::pair<const int, petfurniture::Instance>, std::__multimap_do_transform<int, petfurniture::Instance, std::less<int>, std::allocator<std::pair<const int, petfurniture::Instance>>, 0>::value_compare, std::allocator<std::pair<const int, petfurniture::Instance>>>::node>&, Metrowerks::compressed_pair<Metrowerks::number<unsigned long, 1>, std::__tree_deleter<std::pair<const int, petfurniture::Instance>, std::__multimap_do_transform<int, petfurniture::Instance, std::less<int>, std::allocator<std::pair<const int, petfurniture::Instance>>, 0>::value_compare, std::allocator<std::pair<const int, petfurniture::Instance>>>::node*>)")]
        [DataRow("skBadString$localstatic3$GetNameByToken__31TTokenSet<18EScriptObjectState>CF18EScriptObjectState", "TTokenSet<EScriptObjectState>::GetNameByToken(EScriptObjectState) const::skBadString")]
        [DataRow("init$localstatic4$GetNameByToken__31TTokenSet<18EScriptObjectState>CF18EScriptObjectState", "TTokenSet<EScriptObjectState>::GetNameByToken(EScriptObjectState) const::localstatic4 guard")]
        [DataRow("@LOCAL@GetAnmPlayPolicy__Q24nw4r3g3dFQ34nw4r3g3d9AnmPolicy@policyTable", "nw4r::g3d::GetAnmPlayPolicy(nw4r::g3d::AnmPolicy)::policyTable")]
        [DataRow("@GUARD@GetAnmPlayPolicy__Q24nw4r3g3dFQ34nw4r3g3d9AnmPolicy@policyTable", "nw4r::g3d::GetAnmPlayPolicy(nw4r::g3d::AnmPolicy)::policyTable guard")]
        [DataRow("lower_bound<Q24rstl180const_pointer_iterator<Q24rstl33pair<Ui,22CAdditiveAnimationInfo>,Q24rstl77vector<Q24rstl33pair<Ui,22CAdditiveAnimationInfo>,Q24rstl17rmemory_allocator>,Q24rstl17rmemory_allocator>,Ui,Q24rstl79pair_sorter_finder<Q24rstl33pair<Ui,22CAdditiveAnimationInfo>,Q24rstl8less<Ui>>>__4rstlFQ24rstl180const_pointer_iterator<Q24rstl33pair<Ui,22CAdditiveAnimationInfo>,Q24rstl77vector<Q24rstl33pair<Ui,22CAdditiveAnimationInfo>,Q24rstl17rmemory_allocator>,Q24rstl17rmemory_allocator>Q24rstl180const_p", null)]
        [DataRow("test__FRCPCPCi", "test(const int* const* const&)")]
        [DataRow("__ct__Q34nw4r2ut14CharStrmReaderFMQ34nw4r2ut14CharStrmReaderFPCvPv_Us", "nw4r::ut::CharStrmReader::CharStrmReader(unsigned short (nw4r::ut::CharStrmReader::*)())")]
        [DataRow("QuerySymbolToMapFile___Q24nw4r2dbFPUcPC12OSModuleInfoUlPUcUl", "nw4r::db::QuerySymbolToMapFile_(unsigned char*, const OSModuleInfo*, unsigned long, unsigned char*, unsigned long)")]
        [DataRow("__ct__Q37JGadget27TLinkList<10JUTConsole,-24>8iteratorFQ37JGadget13TNodeLinkList8iterator", "JGadget::TLinkList<JUTConsole, -24>::iterator::iterator(JGadget::TNodeLinkList::iterator)")]
        [DataRow("do_assign<Q23std126__convert_iterator<PP16GAM_eEngineState,Q33std68__cdeque<P16GAM_eEngineState,36ubiSTLAllocator<P16GAM_eEngineState>>8iterator>>__Q23std36__cdeque<PCv,20ubiSTLAllocator<PCv>>FQ23std126__convert_iterator<PP16GAM_eEngineState,Q33std68__cdeque<P16GAM_eEngineState,36ubiSTLAllocator<P16GAM_eEngineState>>8iterator>Q23std126__convert_iterator<PP16GAM_eEngineState,Q33std68__cdeque<P16GAM_eEngineState,36ubiSTLAllocator<P16GAM_eEngineState>>8iterator>Q23std20forward_iterator_tag", "std::__cdeque<const void*, ubiSTLAllocator<const void*>>::do_assign<std::__convert_iterator<GAM_eEngineState**, std::__cdeque<GAM_eEngineState*, ubiSTLAllocator<GAM_eEngineState*>>::iterator>>(std::__convert_iterator<GAM_eEngineState**, std::__cdeque<GAM_eEngineState*, ubiSTLAllocator<GAM_eEngineState*>>::iterator>, std::__convert_iterator<GAM_eEngineState**, std::__cdeque<GAM_eEngineState*, ubiSTLAllocator<GAM_eEngineState*>>::iterator>, std::forward_iterator_tag)")]
        [DataRow("__opPCQ23std15__locale_imp<1>__Q23std80_RefCountedPtr<Q23std15__locale_imp<1>,Q23std32_Single<Q23std15__locale_imp<1>>>CFv", "std::_RefCountedPtr<std::__locale_imp<1>, std::_Single<std::__locale_imp<1>>>::operator const std::__locale_imp<1>*() const")]
        [DataRow("__partition_const_ref<PP12CSpaceObject,Q23std74unary_negate<Q23std52__binder1st_const_ref<Q23std21less<P12CSpaceObject>>>>__3stdFPP12CSpaceObjectPP12CSpaceObjectRCQ23std74unary_negate<Q23std52__binder1st_const_ref<Q23std21less<P12CSpaceObject>>>", "std::__partition_const_ref<CSpaceObject**, std::unary_negate<std::__binder1st_const_ref<std::less<CSpaceObject*>>>>(CSpaceObject**, CSpaceObject**, const std::unary_negate<std::__binder1st_const_ref<std::less<CSpaceObject*>>>&)")]
        public void TestDemangle(string mangled, string demangled)
        {
            DemangleOptions options = new DemangleOptions();

            string result = CWDemangler.demangle(mangled, options);

            if (result != null)
            {
                Console.WriteLine("Target: " + demangled);
                Console.WriteLine("Actual: " + result);
            }
            else
            {
                Console.WriteLine("Demangling failed");
            }

            Assert.AreEqual(demangled, result);
        }

        [TestMethod]
        [DataRow(true, false, "__dt__26__partial_array_destructorFv", "__partial_array_destructor::~__partial_array_destructor()")]
        [DataRow(false, false, "__dt__26__partial_array_destructorFv", "__partial_array_destructor::~__partial_array_destructor(void)")]
        [DataRow(true, true, "__opPCQ23std15__locale_imp<1>__Q23std80_RefCountedPtr<Q23std15__locale_imp<1>,Q23std32_Single<Q23std15__locale_imp<1>>>CFv", "std::_RefCountedPtr<std::__locale_imp<__int128>, std::_Single<std::__locale_imp<__int128>>>::operator const std::__locale_imp<__int128>*() const")]
        [DataRow(true, true, "fn<3,PV2>__FPC2", "fn<3, volatile __vec2x32float__*>(const __vec2x32float__*)")]
        public void TestDemangleOptions(bool omitEmptyParams, bool mwExtensions, string mangled, string demangled)
        {
            DemangleOptions options = new DemangleOptions(omitEmptyParams, mwExtensions);

            string result = CWDemangler.demangle(mangled, options);

            Assert.AreEqual(demangled, result);
        }
    }
}