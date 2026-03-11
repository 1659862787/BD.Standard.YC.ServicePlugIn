using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BD.Standard.YC.ServicePlugIn.ProjectInitiation
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("项目立项(变更)单审核操作服务插件")]
    public class AuditOperationPlugIn : AbstractOperationServicePlugIn
    {
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
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// //
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                DynamicObject entity = e.DataEntitys[0];
                //根据单据ID获取单据编号
                var formid = BusinessInfo.GetForm().Id.ToString();
                //根据不同的单据类型获取单据编号，并调用保存预制单据数据的方法
                if (StringUtils.EqualsIgnoreCase(formid, "UJED_projectInitiation"))
                {
                    string fbillno = entity["billno"].ToString();
                    SavePreBaseData(entity, fbillno);

                }
                if (StringUtils.EqualsIgnoreCase(formid, "UJED_projectInitiationChange"))
                {
                    string fbillno = entity["billno"].ToString();
                    fbillno = fbillno.Substring(0, fbillno.LastIndexOf("_"));
                    SavePreBaseData(entity, fbillno);

                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void SavePreBaseData(DynamicObject entity, string fbillno)
        {
            //根据单据编号查询预制单据ID，如果存在则更新，不存在则新增
            string sql = $"select fid from T_BAS_PREBDTWO WHERE FNUMBER='{fbillno}'";
            long fid = DBUtils.ExecuteScalar<long>(this.Context, sql, 0, null);
            JObject id = new JObject()
                    {
                        new JProperty("fid",fid),
                        new JProperty("fnumber",fbillno),
                        new JProperty("fname",entity["F_ProjectName"].ToString()),
                    };
            JObject jsons = new JObject()
                    {
                        new JProperty("IsAutoSubmitAndAudit",fid==0?true:false),
                        new JProperty("model",id),
                    };
            string MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Save(Context, "BAS_PreBaseDataTwo", JsonConvert.SerializeObject(jsons)));
            if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
            {
            }
            else
            {
                throw new Exception(JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["Errors"][0]["Message"].ToString());
            }
        }
    }

}
