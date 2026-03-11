using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BD.Standard.YC.ServicePlugIn.OtherStockOut
{
    [Description("其他出库单表单校验插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class OtherStockOutBillPlugin : AbstractDynamicFormPlugIn
    {


        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
           
        }

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);
            try
            {
                // 在提交操作时，校验出库金额是否超过项目的原材料预算
                if (e.Operation.FormOperation.Operation.EqualsIgnoreCase("Submit"))
                {
                    decimal totalAmount = 0;
                    foreach (var item in this.View.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity")))
                    {
                        totalAmount += Convert.ToDecimal(item["Amount"]);
                    }

                    DynamicObject[] UJED_ProjectInitiation = GetFmaterial();
                    if (UJED_ProjectInitiation != null && UJED_ProjectInitiation.Length > 0)
                    {
                        decimal materialBudget = Convert.ToDecimal(UJED_ProjectInitiation[0]["F_material"]);
                        string F_ProjectType = Convert.ToString(UJED_ProjectInitiation[0]["F_ProjectType"]);
                        if (totalAmount > materialBudget&& F_ProjectType.Equals("YF"))
                        {
                            e.Cancel = true;
                            this.View.ShowErrMessage($"出库总成本({totalAmount})已超出项目原材料预算({materialBudget})，不允许提交单据！");
                            return;
                        }
                    }
                    else
                    {
                        e.Cancel = true;
                        this.View.ShowErrMessage("项目编码无法查询到对应的项目立项单，无法完成校验！");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private DynamicObject[] GetFmaterial()
        {
            DynamicObject dynamicObject = (DynamicObject)this.View.Model.GetValue("F_ProjectInitiation");
            if (dynamicObject != null)
            {
                string F_ProjectInitiation = dynamicObject["Number"].ToString();
                List<SelectorItemInfo> lstSelectorItemInfos = new List<SelectorItemInfo>();
                lstSelectorItemInfos.Add(new SelectorItemInfo("F_material"));
                lstSelectorItemInfos.Add(new SelectorItemInfo("F_ProjectType"));

                DynamicObject[] UJED_ProjectInitiation = BusinessDataServiceHelper.Load(Context, "UJED_projectInitiation", lstSelectorItemInfos, OQLFilter.CreateHeadEntityFilter("Fbillno = '" + F_ProjectInitiation + "'"));
                return UJED_ProjectInitiation;
            }

            return null;
        }
    }
}
