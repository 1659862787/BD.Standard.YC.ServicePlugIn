//using Kingdee.BOS.Core.DynamicForm;
//using Kingdee.BOS.Core.DynamicForm.PlugIn;
//using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
//using Kingdee.BOS.Core.Metadata;
//using Kingdee.BOS.Orm.DataEntity;
//using Kingdee.BOS.ServiceHelper;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;

//namespace BD.Standard.YC.ServicePlugIn.Requisition
//{
//    [Kingdee.BOS.Util.HotUpdate]
//    [Description("采购申请保存操作服务插件")]
//    public class SaveOperationPlugIn : AbstractOperationServicePlugIn
//    {
//        public override void OnPreparePropertys(PreparePropertysEventArgs e)
//        {
//            base.OnPreparePropertys(e);
//            List<Kingdee.BOS.Core.Metadata.FieldElement.Field> fields = this.BusinessInfo.GetFieldList();
//            foreach (var item in fields)
//            {
//                e.FieldKeys.Add(item.Key);
//            }
//        }

//        /// <summary>
//        /// 审核按钮集合方法
//        /// </summary>
//        /// <param name="e"></param>
//        /// //
//        public override void EndOperationTransaction(EndOperationTransactionArgs e)
//        {
//            base.EndOperationTransaction(e);
//            try
//            {
//                IOperationResult operationResult = new OperationResult();
//                foreach (DynamicObject entity in e.DataEntitys)
//                {
//                    string fid = entity[0].ToString();
//                    string srcid = entity["F_SRCID"].ToString();
//                    //获取源单数据
//                    FormMetadata ExpMeta = MetaDataServiceHelper.Load(this.Context, "UJED_projectInitiation", true) as FormMetadata;
//                    DynamicObject Expobj = BusinessDataServiceHelper.LoadSingle(this.Context, srcid, ExpMeta.BusinessInfo.GetDynamicObjectType());
//                    DynamicObjectCollection srcdyc = Expobj["FEntity"] as DynamicObjectCollection;

//                    #region 表头实体赋值
//                    Expobj["F_Cutomer"] = entity["F_Cutomer"];
//                    Expobj["F_Cutomer_Id"] = Convert.ToInt64(entity["F_Cutomer_Id"]);
//                    Expobj["F_ProjectName"] = entity["F_ProjectName"].ToString();
//                    Expobj["F_ProjectAmount"] = entity["F_ProjectAmount"].ToString();
//                    Expobj["F_ProjectType"] = entity["F_ProjectType"].ToString();
//                    Expobj["F_ProjectStatus"] = entity["F_ProjectStatus"].ToString();
//                    Expobj["F_material"] = entity["F_material"].ToString();
//                    Expobj["F_manual"] = entity["F_manual"].ToString();
//                    Expobj["F_cost"] = entity["F_cost"].ToString();
//                    Expobj["F_market"] = entity["F_market"];
//                    Expobj["F_market_Id"] = Convert.ToInt64(entity["F_market_Id"]);
//                    Expobj["F_technology"] = entity["F_technology"];
//                    Expobj["F_technology_Id"] = Convert.ToInt64(entity["F_technology_Id"]);
//                    Expobj["F_supplyChain"] = entity["F_supplyChain"];
//                    Expobj["F_supplyChain_Id"] = entity["F_supplyChain_Id"];
//                    Expobj["F_BeginDate"] = entity["F_BeginDate"].ToString();
//                    Expobj["F_EndDate"] = entity["F_EndDate"].ToString();
//                    Expobj["F_rate"] = entity["F_rate"].ToString();
//                    Expobj["F_IsChange"] = "true";
//                    Expobj["F_newVersion"] = entity["F_newVersion"].ToString();
//                    Expobj["F_ChangeReason"] = entity["F_ChangeReason"].ToString();

                    
//                    #endregion 表头实体赋值

//                    DynamicObjectCollection dyc = entity["FEntity"] as DynamicObjectCollection;
//                    foreach (DynamicObject item in dyc)
//                    {
//                        string combo = item["F_ChangeType"].ToString();
//                        switch (combo)
//                        {
//                            //新增明细
//                            case "A":
//                                if (item["F_Material1"] != null)
//                                {
//                                    DynamicObject srcdy = srcdyc.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
//                                    srcdy["Seq"] = srcdyc.Count + 1;
//                                    srcdy["F_Material1"] = item["F_Material1"];
//                                    long materialId = Convert.ToInt64(item["F_Material1_Id"]);
//                                    srcdy["F_Material1_Id"] = materialId;
//                                    srcdy["F_Price"] = item["F_Price"];
//                                    srcdy["F_taxRate"] = item["F_taxRate"];
//                                    srcdy["F_taxPrice"] = item["F_taxPrice"];
//                                    srcdy["F_Amount"] = item["F_Amount"];
//                                    srcdy["F_TaxAmount"] = item["F_TaxAmount"];
//                                    srcdyc.Add(srcdy);
//                                }
//                                break;
//                            //修改明细
//                            case "B":
//                                long srcentryid = Convert.ToInt64(item["F_srcentryid"]);
//                                foreach (DynamicObject srcdy in srcdyc)
//                                {
//                                    if (Convert.ToInt64(srcdy["id"]) == srcentryid)
//                                    {
//                                        srcdy["F_Price"] = item["F_Price"];
//                                        srcdy["F_taxRate"] = item["F_taxRate"];
//                                        srcdy["F_taxPrice"] = item["F_taxPrice"];
//                                        srcdy["F_Amount"] = item["F_Amount"];
//                                        srcdy["F_TaxAmount"] = item["F_TaxAmount"];
//                                        break;
//                                    }
//                                }
//                                break;
//                            //删除明细
//                            case "D":
//                                long srcentryid1 = Convert.ToInt64(item["F_srcentryid"]);
//                                foreach (DynamicObject srcdy in srcdyc)
//                                {
//                                    if (Convert.ToInt64(srcdy["id"]) == srcentryid1)
//                                    {
//                                        srcdyc.Remove(srcdy);
//                                        BusinessDataServiceHelper.Save(this.Context, Expobj);
//                                        break;
//                                    }
//                                }
//                                break;

//                        }


//                    }
//                    BusinessDataServiceHelper.Save(this.Context, Expobj);

//                }
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);
//            }
//        }
//    }
//}
