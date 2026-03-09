using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BD.Standard.YC.ServicePlugIn.Requisition
{
    [Description("采购申请表单_插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PurRequisitionBillPlugin : AbstractDynamicFormPlugIn
    {
        private bool doSave = false;
        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);

            try
            {
                if (e.Operation.FormOperation.Operation.EqualsIgnoreCase("Save"))
                {
                    decimal totalAmount = 0;
                    foreach (var item in this.View.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity")))
                    {
                        totalAmount += Convert.ToDecimal(item["Amount"]);
                    }
                    DynamicObject dynamicObject = (DynamicObject)this.View.Model.GetValue("F_ProjectInitiation");
                    if (dynamicObject != null)
                    {
                        string F_ProjectInitiation = dynamicObject["Number"].ToString();

                        List<SelectorItemInfo> lstSelectorItemInfos = new List<SelectorItemInfo>();
                        lstSelectorItemInfos.Add(new SelectorItemInfo("F_material"));

                        DynamicObject[] UJED_ProjectInitiation = BusinessDataServiceHelper.Load(Context, "UJED_projectInitiation", lstSelectorItemInfos, OQLFilter.CreateHeadEntityFilter("Fbillno = '" + F_ProjectInitiation + "'"));
                        if (UJED_ProjectInitiation.Count() > 0)
                        {
                            decimal subAmount = decimal.Subtract(totalAmount, Convert.ToDecimal(UJED_ProjectInitiation[0]["F_material"]));
                            if (subAmount >= 0)
                            {
                               
                                if (doSave)
                                {
                                    doSave = false;
                                    return;
                                }
                                e.Cancel = true;
                                this.View.ShowMessage("采购的金额是否已经超出原材料的预算,继续的话则需要录入额外申请预算的金额：确认保存？", MessageBoxOptions.OKCancel, result =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        // 继续保存操作
                                        doSave=true;
                                        //this.View.GetFieldEditor("F_UJED_Decimal_qtr", 0). = true;
                                        this.View.GetControl("F_UJED_Decimal_qtr").SetCustomPropertyValue("MustInput", true);

                                        this.View.InvokeFormOperation("Save");

                                    }
                                });

                            }
                        }
                        else
                        {
                            //throw new KDException("查询项目立项单失败", "无法查询到对应的项目立项单！");
                            this.View.ShowErrMessage("项目编码无法查询到对应的项目立项单！");
                            e.Cancel = true;    
                        }



                    }



                }
            }
            catch (Exception ex)
            {

            }

            
            
        }


    }
}
