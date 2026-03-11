using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BD.Standard.YC.ServicePlugIn.Requisition
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("采购申请审核与反审核操作服务插件")]
    public class OperationPlugIn : AbstractOperationServicePlugIn
    {

        private string F_BUDGET = "F_UJED_Decimal_qtr";
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Kingdee.BOS.Core.Metadata.FieldElement.Field> fields = this.BusinessInfo.GetFieldList();
            foreach (var item in fields)
            {
                e.FieldKeys.Add(item.Key);
            }
        }

        /// <summary>
        /// 审核按钮集合方法
        /// </summary>
        /// <param name="e"></param>
        /// //
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();
                    decimal f_budget = Convert.ToDecimal(entity[F_BUDGET]);
                    string F_ProjectInitiation = ((DynamicObject)entity["F_ProjectInitiation"])["Number"].ToString();

                    string operation = this.FormOperation.Operation;
                    //审核
                    if (operation.EqualsIgnoreCase("Audit"))
                    {
                        DBUtils.Execute(this.Context, string.Format("update t_projectInitiation set F_material=F_material+{1} where fbillno='{0}'", F_ProjectInitiation, f_budget));

                    }
                    else if (operation.EqualsIgnoreCase("UnAudit"))
                    {
                        //反审核
                        DBUtils.Execute(this.Context, string.Format("update t_projectInitiation set F_material=F_material-{1} where fbillno='{0}'", F_ProjectInitiation, f_budget));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
