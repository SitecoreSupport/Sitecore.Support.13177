namespace Sitecore.Support.XA.Foundation.LocalDatasources.CustomFields.FieldTypesEx
{
  using Sitecore;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines;
  using Sitecore.Pipelines.GetRenderingDatasource;
  using Sitecore.Web;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.XA.Foundation.LocalDatasources.Models;
  using Sitecore.XA.Foundation.Presentation.Layout;
  using System;
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using System.Linq;

  public class BucketInternalLink : Sitecore.XA.Foundation.LocalDatasources.CustomFields.FieldTypesEx.BucketInternalLink
  {
    private Database ContentDatabase => Client.ContentDatabase;
    protected override void ShowDialog(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Item item = RenderingItem;
      if (item == null)
      {
        item = ContentItem;
      }
      Item contentItem = ContentItem;
      string value = Value;
      GetRenderingDatasourceArgs getRenderingDatasourceArgs = new GetRenderingDatasourceArgs(item)
      {
        FallbackDatasourceRoots = new List<Item>
            {
                ContentDatabase.GetRootItem()
            },
        ContentLanguage = contentItem?.Language,
        ContextItemPath = ((contentItem != null) ? contentItem.Paths.FullPath : string.Empty),
        ShowDialogIfDatasourceSetOnRenderingItem = true,
        CurrentDatasource = value
      };
      EditRenderingPropertiesParameters editRenderingPropertiesParameters = GetEditRenderingPropertiesParameters(contentItem);
      getRenderingDatasourceArgs.CustomData.Add("EditRenderingPropertiesParameters", editRenderingPropertiesParameters);
      CorePipeline.Run("getRenderingDatasource", getRenderingDatasourceArgs);
      if (string.IsNullOrEmpty(getRenderingDatasourceArgs.DialogUrl))
      {
        SheerResponse.Alert("An error occurred.", Array.Empty<string>());
      }
      else
      {
        SheerResponse.ShowModalDialog(getRenderingDatasourceArgs.DialogUrl, "1200px", "700px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    protected override EditRenderingPropertiesParameters GetEditRenderingPropertiesParameters(Item contentItem)
    {
      NameValueCollection parametersFromSuspendedPipeline = GetParametersFromSuspendedPipeline();
      string text = parametersFromSuspendedPipeline["handle"];
      LayoutModel layoutModel = (text == null) ? new LayoutModel(contentItem) : new LayoutModel(WebUtil.GetSessionValue(text).ToString());
      ID deviceId = new ID(parametersFromSuspendedPipeline["device"]);
      List<RenderingModel> renderingsCollection = layoutModel.Devices[deviceId].Renderings.RenderingsCollection;
      string value = parametersFromSuspendedPipeline["selectedindex"];
      if (renderingsCollection.Any() && !string.IsNullOrWhiteSpace(value))
      {
        int index = int.Parse(parametersFromSuspendedPipeline["selectedindex"]);
        RenderingModel renderingModel = renderingsCollection[index];
        return new EditRenderingPropertiesParameters
        {
          DeviceId = deviceId,
          Layout = layoutModel.ToString(),
          Placeholder = renderingModel.Placeholder
        };
      }
      return new EditRenderingPropertiesParameters
      {
        DeviceId = deviceId,
        Layout = layoutModel.ToString(),
        Placeholder = string.Empty
      };
    }
  }
}