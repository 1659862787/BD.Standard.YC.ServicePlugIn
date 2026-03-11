using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BD.Standard.YC.ServicePlugIn.ProjectInitiation
{
    [Description("项目立项表单_插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ProduceBillPlugin : AbstractDynamicFormPlugIn
    {

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);

            string formid = View.UserParameterKey;

            //跳转BOM界面：按钮绑定操作
            //&&StringUtils.EqualsIgnoreCase(formid, "UJED_projectInitiation")
            if (e.Operation.FormOperation.Operation.EqualsIgnoreCase("DoNothing1"))
            {
                int row = this.Model.GetEntryCurrentRowIndex("FEntity");
                DynamicObject F_Material1 = (DynamicObject)this.View.Model.GetValue("F_Material1", row);
                List<DynamicObject> dynamicObjects = HighVersionBomDatas.HighVersionBomData(this.Context, Convert.ToInt64(F_Material1[0]), 1, 0);
                if (dynamicObjects.Count > 0)
                {
                    BillShowParameter bill = new BillShowParameter();
                    bill.FormId = "ENG_BOM";
                    bill.PKey = dynamicObjects[0]["ID"].ToString();
                    bill.OpenStyle.ShowType = ShowType.MainNewTabPage;
                    bill.SyncCallBackAction = true;
                    bill.Status = OperationStatus.EDIT;
                    this.View.ShowForm(bill);
                }
            }

        }

        public static class HighVersionBomDatas
        {
            public static List<DynamicObject> HighVersionBomData(Context Context, long FMaterialId, long OrgId, long auxPropId)
            {
                List<Tuple<long, long, long>> dicMasterOrgIds = new List<Tuple<long, long, long>>();
                dicMasterOrgIds.Add(new Tuple<long, long, long>(FMaterialId, OrgId, auxPropId));
                List<DynamicObject> highVersionBomDatas = BOMServiceHelper.GetHightVersionBom(Context, dicMasterOrgIds).ToList();
                return highVersionBomDatas;
            }

        }

    }
}
