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
    /// <summary>
    /// 采购申请审核与反审核操作服务插件。
    /// 在采购申请单据执行审核或反审核操作时，更新对应立项单的物料预算字段。
    /// </summary>
    public class OperationPlugIn : AbstractOperationServicePlugIn
    {

        /// <summary>
        /// 预算字段的业务字段名，表示申请单上的预算金额字段。
        /// </summary>
        private string F_BUDGET = "F_UJED_Decimal_qtr";
        /// <summary>
        /// 在插件准备字段属性时，注册当前业务单据的所有字段，以便在后续事件中可以使用这些字段值。
        /// </summary>
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
                // 遍历所有操作涉及的数据实体（单据）
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    // 主键或第一个列的字符串表示（根据业务可能不是必须）
                    string fid = entity[0].ToString();

                    // 从单据中读取预算金额字段并转换为 decimal
                    decimal f_budget = Convert.ToDecimal(entity[F_BUDGET]);

                    // 获取关联的立项单号（若存在）
                    string F_ProjectInitiation = null;
                    if (entity["F_ProjectInitiation"] is DynamicObject proj && proj["Number"] != null)
                    {
                        F_ProjectInitiation = proj["Number"].ToString();
                    }

                    // 当前触发的单据操作（例如 Audit 或 UnAudit）
                    string operation = this.FormOperation.Operation;

                    // 审核操作：将申请单预算加到对应立项单的 F_material 字段
                    if (operation.EqualsIgnoreCase("Audit"))
                    {
                        DBUtils.Execute(this.Context, string.Format("update t_projectInitiation set F_material=F_material+{1} where fbillno='{0}'", F_ProjectInitiation, f_budget));

                    }
                    else if (operation.EqualsIgnoreCase("UnAudit"))
                    {
                        // 反审核操作：从立项单的 F_material 字段扣除申请单的预算
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
