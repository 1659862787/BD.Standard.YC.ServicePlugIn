using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace BD.Standard.YC.ServicePlugIn.ProjectInitiationX
{
    [Description("项目生产订货变更单_插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ProduceXBillPlugin : AbstractDynamicFormPlugIn
    {


        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            EntryEntity entry = this.View.BillBusinessInfo.GetEntryEntity("FEntity");
            DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entry) as DynamicObjectCollection;
            if (dy != null)
            {
                for (int row = 0; row < dy.Count; row++)
                {
                    
                    if (dy[row]["F_ChangeType"].ToString().Equals("A"))
                    {
                        this.View.GetFieldEditor("F_ChangeType", row).Enabled = false;
                    }
                    else
                    {
                        this.View.GetFieldEditor("F_material1", row).Enabled = false;
                    }

                }
            }


        }

        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);
            //获取源单id
            string v = View.Model.GetValue("F_newVersion").ToString();
            int F_newVersion = Convert.ToInt32(this.View.Model.GetValue("F_newVersion").ToString());
            string billno = this.View.Model.GetValue("FBILLNO").ToString();
                int i = F_newVersion;
            if ((++F_newVersion).ToString().Length == 1)
            {
                this.Model.DataObject["BillNo"] = billno + "_V00" + ++i;
                this.Model.DataObject["F_newVersion"] ="00" + i;
            }
            if ((++F_newVersion).ToString().Length == 2)
            {
                this.Model.DataObject["BillNo"] = billno + "_V0" + ++i;
                this.Model.DataObject["F_newVersion"] = "0" + i;
            }
            if ((++F_newVersion).ToString().Length == 3)
            {
                this.Model.DataObject["BillNo"] = billno + "_V" + ++i;
                this.Model.DataObject["F_newVersion"] = i;
            }
        }

        /// <summary>
        /// 新增行数据
        /// </summary>
        /// <param name="e"></param>
        public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
        {
            base.AfterCreateNewEntryRow(e);
            this.View.Model.SetValue("F_ChangeType", "A", e.Row);
            this.View.Model.SetValue("F_srcentryid", "0", e.Row);
            this.View.GetFieldEditor("F_ChangeType", e.Row).Enabled = false;
            // this.View.Refresh();
        }



        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            int row = e.Row;
            if (e.Field.FieldName.EqualsIgnoreCase("F_ChangeType"))
            {
                string F_ChangeType = e.NewValue.ToString();
                string F_ChangeTypeOld = e.OldValue.ToString();
                if (F_ChangeType.Equals("A")&& !F_ChangeTypeOld.Equals("A") && !string.IsNullOrWhiteSpace(this.View.Model.GetValue("F_srcentryid", e.Row)!=null? this.View.Model.GetValue("F_srcentryid", e.Row).ToString():""))
                {
                    DynamicObjectCollection dyc = this.Model.DataObject["FEntity"] as DynamicObjectCollection;
                    foreach (var item in dyc)
                    {
                        if (Convert.ToInt32(item["seq"]) - 1 == row)
                        {
                            item["F_ChangeType"] = 'B';
                        }
                    }
                    this.View.UpdateView("F_ChangeType");
                    this.View.ShowErrMessage("源单明细数据，不允许新增！");
                   
                }



            }
           

        }

        /// <summary>
        /// 删除明细行前
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
        {
            base.BeforeDeleteRow(e);
            if (e.EntityKey.EqualsIgnoreCase("FEntity"))
            {
                //获取删除明细行的已用数量字段
                object v = this.View.Model.GetValue("F_srcentryid", e.Row);
                int F_srcentryid = Convert.ToInt32(this.View.Model.GetValue("F_srcentryid", e.Row));
                if (F_srcentryid > 0)
                {
                    this.View.ShowErrMessage("源单明细已有下游单据，禁止删除分录行！");
                    e.Cancel = true;
                   
                }
            }
        }

        /// <summary>
        /// 删除明细行后数据反写表头删除明细id字段
        /// </summary>
        /// <param name="e"></param>
        public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
        {
            base.AfterDeleteRow(e);
        }

        /// <summary>
        /// 修改明细
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            base.BeforeUpdateValue(e);

        }

    }
}
