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

namespace BD.Standard.YC.ServicePlugIn.Requisition
{
    [Description("采购申请表单_插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PurRequisitionBillPlugin : AbstractDynamicFormPlugIn
    {
        private bool doSave = false;
        //额外申请预算
        private string F_BUDGET = "F_UJED_Decimal_qtr";

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            //当单据打开时，如果金额超出预算则显示额外申请预算的字段，并设置为必输项
            decimal subAmount = Convert.ToDecimal(View.Model.GetValue(F_BUDGET, 0));
            if (subAmount > 0)
            {
                this.View.GetFieldEditor(F_BUDGET, 0).Visible = true;
                this.View.GetControl(F_BUDGET).SetCustomPropertyValue("MustInput", true);
            }
            else
            {
                this.View.GetFieldEditor(F_BUDGET, 0).Visible = false;
                this.View.GetControl(F_BUDGET).SetCustomPropertyValue("MustInput", false);
            }

        }

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);
            try
            {
                //当保存操作时，进行金额校验，如果超出预算则提示用户是否继续保存，并录入额外申请预算的金额
                if (e.Operation.FormOperation.Operation.EqualsIgnoreCase("Submit"))
                {
                    decimal totalAmount = 0;
                    foreach (var item in this.View.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity")))
                    {
                        totalAmount += Convert.ToDecimal(item["Amount"]);
                    }
                    DynamicObject[] UJED_ProjectInitiation = GetFmaterial();
                    //如果查询到项目立项单，则进行金额校验
                    if (UJED_ProjectInitiation.Count() > 0)
                    {
                        //计算超出预算的金额
                        decimal subAmount = decimal.Subtract(totalAmount, Convert.ToDecimal(UJED_ProjectInitiation[0]["F_material"])) - Convert.ToDecimal(View.Model.GetValue(F_BUDGET, 0));
                        if (subAmount > 0)
                        {
                            //如果已经提示过一次，并且用户选择继续保存，则不再提示，直接保存
                            if (doSave)
                            {
                                doSave = false;
                                return;
                            }
                            e.Cancel = true;
                            this.View.ShowMessage($"采购的金额已经超出原材料的预算，超出额度:{subAmount},继续的话则需要录入额外申请预算的金额！\r\n是否继续？", MessageBoxOptions.OKCancel, result =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    //用户选择继续保存，则显示额外申请预算的字段，并设置为必输项
                                    doSave = true;
                                    this.View.GetFieldEditor(F_BUDGET, 0).Visible = true;
                                    this.View.GetControl(F_BUDGET).SetCustomPropertyValue("MustInput", true);
                                    View.Model.SetValue(F_BUDGET, subAmount);
                                    this.View.InvokeFormOperation("Save");
                                    this.View.InvokeFormOperation("Submit");

                                }
                            });
                            this.View.InvokeFormOperation("Save");
                        }
                    }
                    else
                    {
                        this.View.ShowErrMessage("项目编码无法查询到对应的项目立项单！");
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 获取项目立项单的原材料预算金额
        /// </summary>
        /// <returns>项目立项单对象</returns>
        private DynamicObject[] GetFmaterial()
        {
            DynamicObject dynamicObject = (DynamicObject)this.View.Model.GetValue("F_ProjectInitiation");
            if (dynamicObject != null)
            {
                string F_ProjectInitiation = dynamicObject["Number"].ToString();
                //查询项目立项单的原材料预算金额
                List<SelectorItemInfo> lstSelectorItemInfos = new List<SelectorItemInfo>();
                lstSelectorItemInfos.Add(new SelectorItemInfo("F_material"));

                DynamicObject[] UJED_ProjectInitiation = BusinessDataServiceHelper.Load(Context, "UJED_projectInitiation", lstSelectorItemInfos, OQLFilter.CreateHeadEntityFilter("Fbillno = '" + F_ProjectInitiation + "'"));
                return UJED_ProjectInitiation;
            }

            return null;
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            try
            {
                int row = e.Row;


                //当修改单据头的项目立项编码时，重新获取原材料预算金额，并进行金额校验
                //if (e.Field.FieldName.EqualsIgnoreCase(F_BUDGET))
                //{
                //    decimal newF_BUDGET = Convert.ToDecimal(e.NewValue.ToString());
                //    decimal oldF_BUDGET = Convert.ToDecimal(e.OldValue.ToString());

                //    decimal totalAmount = 0;
                //    decimal subAmount = 0;
                //    foreach (var item in this.View.Model.GetEntityDataObject(this.Model.BusinessInfo.GetEntity("FEntity")))
                //    {
                //        totalAmount += Convert.ToDecimal(item["Amount"]);
                //    }
                //    DynamicObject[] UJED_ProjectInitiation = GetFmaterial();
                //    if (UJED_ProjectInitiation.Count() > 0)
                //    {
                //        subAmount = decimal.Subtract(totalAmount, Convert.ToDecimal(UJED_ProjectInitiation[0]["F_material"]));
                //    }

                //    if (oldF_BUDGET > 0 && decimal.Subtract(newF_BUDGET, subAmount) < 0)
                //    {
                //        DynamicObject dataObject = this.View.Model.DataObject;
                //        dataObject[F_BUDGET] = oldF_BUDGET;
                //        this.View.ShowErrMessage($"额外申请预算的金额能小于超出额度！目前单据金额超出:{subAmount}!请重新录入额外申请预算！");
                //        View.UpdateView(F_BUDGET);
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }






        }
    }
}
