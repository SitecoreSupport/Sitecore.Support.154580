using Sitecore.ContentTesting.Diagnostics;
using Sitecore.ContentTesting.Screenshot;
using Sitecore.Diagnostics;
using Sitecore.Web.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Sitecore.Support.ContentTesting.Screenshot
{
    public class ScreenshotContextFactory : Sitecore.ContentTesting.Screenshot.ScreenshotContextFactory
    {
        public override void DestroyTemporarySession(TemporarySessionInfo session)
        {
            if (session != null)
            {
                if (!string.IsNullOrEmpty(session.Ticket))
                {
                    TicketManager.RemoveTicket(session.Ticket);
                }
                if (session.User != null)
                {
                    session.User.Delete();
                }
                session.TemporaryHttpContext = null;
            }

            Thread.Sleep(5000);
            if (DomainAccessGuard.Sessions == null)
                return;

            foreach (DomainAccessGuard.Session DagSession in DomainAccessGuard.Sessions)
            {
                if (DagSession.UserName == "sitecore\\virtualssuser")
                {
                    try
                    {
                        DomainAccessGuard.Kick(DagSession.SessionID);
                        Log.Info("Sitecore.Support.154580: sitecore\\virtualssuser has been kicked", this);            
                    }
                    catch(Exception ex)
                    {
                        Log.Error("Sitecore.Support.154580: " + ex.Message, this);
                    }
                }
            }
        }
    }
}