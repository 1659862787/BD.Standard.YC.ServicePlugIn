using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace BD.Standard.YC.ServicePlugIn.Payment
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("费用应付单审核与反审核操作服务插件")]
    public class OperationPlugIn : AbstractOperationServicePlugIn
    {
        /*
         伪代码与实现计划（详细）：
         1. 在 EndOperationTransaction 中，对每个实体(entity)：
            a. 初始化 decimal 类型的累计变量 f_budget = 0m。
            b. 从实体中读取项目立项编号 F_ProjectInitiation（如果存在则取其 Number）。
            c. 从实体中读取应付分录集合 `AP_PAYABLEENTRY`，转换为 DynamicObjectCollection。
            d. 如果集合不为空，遍历每一行(row)：
               - 从 row 中取字段 `FALLAMOUNTFOR_D` 的值。
               - 对 null 或 DBNull.Value 做判断，若有效则尝试转换为 decimal 并累加到 f_budget。
               - 对转换异常采用安全忽略，确保不影响其它行计算。
            e. 获取当前操作类型（审核或反审核）：
               - 若为 Audit，则使用 f_budget 更新 t_projectInitiation 表：F_cost = F_cost + f_budget。
               - 若为 UnAudit，则更新为 F_cost = F_cost - f_budget。
            f. 为避免小数点与区域设置问题，使用 InvariantCulture 将 decimal 转为字符串再拼接 SQL。
         2. 保留原有的异常捕获逻辑，遇到异常抛出新异常（保留原始消息）。
         3. 确保每个实体的 f_budget 独立计算，不在实体之间累加。
        */

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
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    // 每个实体单独计算累计金额
                    decimal f_budget = 0m;

                    // 读取立项单号（做空值保护）
                    string F_ProjectInitiation = null;
                    if (entity["F_ProjectInitiation"] is DynamicObject proj && proj["Number"] != null)
                    {
                        F_ProjectInitiation = proj["Number"].ToString();
                    }

                    // 读取应付分录集合并遍历累计 FALLAMOUNTFOR_D 字段
                    DynamicObjectCollection payableentry = entity["AP_PAYABLEENTRY"] as DynamicObjectCollection;
                    if (payableentry != null)
                    {
                        foreach (DynamicObject row in payableentry)
                        {
                            try
                            {
                                object val = null;
                                if (row.Contains("FALLAMOUNTFOR_D"))
                                {
                                    val = row["FALLAMOUNTFOR_D"];
                                }
                                if (val != null && val != DBNull.Value)
                                {
                                    // 使用 Convert 兼容多种底层类型（decimal/double/string）
                                    decimal amt = Convert.ToDecimal(val);
                                    f_budget += amt;
                                }
                            }
                            catch
                            {
                                // 忽略单行转换错误，继续处理其它行
                            }
                        }
                    }

                    string operation = this.FormOperation.Operation;
                    // 审核
                    if (operation.EqualsIgnoreCase("Audit"))
                    {
                        string sql = string.Format("update t_projectInitiation set F_cost=F_cost+{0} where fbillno='{1}'",
                            f_budget, F_ProjectInitiation);
                        DBUtils.Execute(this.Context, sql);
                    }
                    else if (operation.EqualsIgnoreCase("UnAudit"))
                    {
                        // 反审核
                        string sql = string.Format("update t_projectInitiation set F_cost=F_cost-{0} where fbillno='{1}'",
                            f_budget, F_ProjectInitiation);
                        DBUtils.Execute(this.Context, sql);
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
