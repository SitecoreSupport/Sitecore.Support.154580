using Sitecore.Common;
using Sitecore.ContentTesting.Configuration;
using Sitecore.ContentTesting.Model.Extensions;
using Sitecore.ContentTesting.Pipelines;
using Sitecore.ContentTesting.Pipelines.GetScreenshot;
using Sitecore.ContentTesting.Pipelines.GetScreenShotForURL;
using Sitecore.ContentTesting.Screenshot;
using Sitecore.ContentTesting.ViewModel;
using Sitecore.Data;
using Sitecore.Globalization;
using Sitecore.Security.Accounts;

namespace Sitecore.Support.ContentTesting.Screenshot
{
    public class ScreenshotGenerator : Sitecore.ContentTesting.Screenshot.ScreenshotGenerator
    {
        protected override ScreenshotUrlModel GenerateScreenshot(ScreenshotTask task)
        {
            if (Switcher<ScreenshotGenerationState, ScreenshotGenerationState>.CurrentValue == ScreenshotGenerationState.Disabled)
            {
                return null;
            }
            GetScreenshotArgs getScreenshotArgs = new GetScreenshotArgs(new DataUri(task.Variation.ItemID, Language.Parse(task.Variation.Language), new Sitecore.Data.Version(task.Variation.ItemVersion ?? Sitecore.Data.Version.Latest.Number)))
            {
                Revision = task.Variation.ItemRevision,
                DeviceId = task.DeviceId,
                Rules = task.Variation.Rules,
                ComponentTestVariants = task.Variation.ActiveMvVariants,
                TestCombination = task.Variation.Combination.MultiplexToString("-"),
                CompareVersion = task.Variation.CompareVersion,
                Cookies = task.Cookies,
                ScaleBounds = task.ScaleBounds,
                ViewPortSize = task.ViewPortSize,
                UrlParameters = task.UrlParameters
            };
            ScreenShotFileNameGenerator screenShotFileNameGenerator = new ScreenShotFileNameGenerator(getScreenshotArgs);
            bool flag = false;
            try
            {
                flag = screenShotFileNameGenerator.SetGenerationLock(60);
                if (flag)
                {
                    UserSwitcher userSwitcher = null;

                    lock (Sitecore.Context.User)
                    {
                        if (task.User != null)
                        {
                            userSwitcher = new UserSwitcher(task.User);
                        }
                        SettingsDependantPipeline<GetScreenshotPipeline, GetScreenshotArgs>.Instance.Run(getScreenshotArgs);
                        if (userSwitcher != null)
                        {
                            userSwitcher.Dispose();
                        }
                    }

                    return new ScreenshotUrlModel
                    {
                        UId = ((!string.IsNullOrEmpty(task.Variation.UId)) ? task.Variation.UId : null),
                        UnscaledUrl = this.ValidateAndMapImage(getScreenshotArgs.UnscaledOutputFilename),
                        Url = this.ValidateAndMapImage(getScreenshotArgs.OutputFilename)
                    };
                }
            }
            finally
            {
                if (flag)
                {
                    screenShotFileNameGenerator.ResetGenerationLock();
                }
            }
            return new ScreenshotUrlModel
            {
                UId = ((!string.IsNullOrEmpty(task.Variation.UId)) ? task.Variation.UId : null),
                UnscaledUrl = "/sitecore/shell/Themes/Standard/Images/warning.png",
                Url = "/sitecore/shell/Themes/Standard/Images/warning.png"
            };
        }
    }
}