﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace WebApiThrottle.Net
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetClientIpAddress(this HttpRequestMessage request)
        {
            // Always return all zeroes for any failure (my calling code expects it)
            string ipAddress = "0.0.0.0";

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                ipAddress = ((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                ipAddress = ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;
            }

            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                ipAddress = ((Microsoft.Owin.OwinContext) request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
            }

            // get the X-Forward-For headers (should only really be one)
            IEnumerable<string> xForwardForList;
            if (!request.Headers.TryGetValues("X-Forwarded-For", out xForwardForList))
            {
               return ipAddress;
            }

            var xForwardedFor = xForwardForList.FirstOrDefault();

            // check that we have a value
            if (string.IsNullOrEmpty(xForwardedFor))
            {
                return ipAddress;
            }

            // Get a list of public ip addresses in the X_FORWARDED_FOR variable
            var publicForwardingIps = xForwardedFor.Split(',');

            // We want the first one, IP addreses after the first one are proxy addresses which we dont want and we also want to remove the port
            if (publicForwardingIps.Any())
            {
                ipAddress = publicForwardingIps.First();

                if (ipAddress.Contains(":"))
                {
                    // Port definition present, remove it
                    return ipAddress.Substring(0, ipAddress.IndexOf(":", StringComparison.Ordinal));
                }
                else
                {
                    return ipAddress;
                }
            }
            else
            {
                return ipAddress;
            } 
        }
    }
}