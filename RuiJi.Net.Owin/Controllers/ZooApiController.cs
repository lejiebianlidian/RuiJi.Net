﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RuiJi.Net.Node;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RuiJi.Net.Owin.Controllers
{
    public class ZooApiController : ApiController
    {
        [HttpGet]
        public object Tree(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = "/";

            var leaderNode = GetLeaderNode();

            if (leaderNode != null)
            {
                var nv = leaderNode.GetChildren(path);

                return nv.AllKeys.Select(m => new { id = m ,text = m.LastIndexOf('/') !=-1 ? m.Substring(m.LastIndexOf('/') + 1) : m, children = nv[m] != "0" });
            }
            else
            {
                var leaderBaseUrl = ServerManager.GetNode(Request.RequestUri.Port.ToString()).NodeBase.LeaderBaseUrl;

                if (string.IsNullOrEmpty(leaderBaseUrl))
                    return null;

                var client = new RestClient("http://" + leaderBaseUrl);
                var restRequest = new RestRequest("api/zoo/tree?path=" + path);
                restRequest.Method = Method.GET;

                var restResponse = client.Execute(restRequest);

                var response = JsonConvert.DeserializeObject<object>(restResponse.Content);

                return response;
            }
        }

        [HttpGet]
        public object NodeData(string path)
        {
            var leaderNode = GetLeaderNode();

            if (leaderNode != null)
            {
                return leaderNode.GetData(path);
            }
            else
            {
                var leaderBaseUrl = ServerManager.GetNode(Request.RequestUri.Port.ToString()).NodeBase.LeaderBaseUrl;

                var client = new RestClient("http://" + leaderBaseUrl);
                var restRequest = new RestRequest("api/zoo/node?path=" + path);
                restRequest.Method = Method.GET;

                var restResponse = client.Execute(restRequest);

                var response = JsonConvert.DeserializeObject<object>(restResponse.Content);

                return response;
            }
        }

        [HttpGet]
        public object Cluster()
        {
            var leaderNode = GetLeaderNode();

            if (leaderNode!=null)
            {
                return leaderNode.GetCluster();
            }
            else
            {
                var leaderBaseUrl = ServerManager.GetNode(Request.RequestUri.Port.ToString()).NodeBase.LeaderBaseUrl;

                var client = new RestClient("http://" + leaderBaseUrl);
                var restRequest = new RestRequest("api/zoo/cluster");
                restRequest.Method = Method.GET;

                var restResponse = client.Execute(restRequest);

                var response = JsonConvert.DeserializeObject<object>(restResponse.Content);

                return response;
            }
        }

        [HttpGet]
        public object GetProxys()
        {
            var leaderNode = GetLeaderNode();

            if (leaderNode != null)
            {
                var results = new Dictionary<string,string>();
                var nv = leaderNode.GetChildren("/live_nodes/proxy");
                foreach (var n in nv.AllKeys)
                {
                    var d = leaderNode.GetData(n);
                    results.Add(d.Data, n.Split('/').Last());
                }

                return results;
            }
            else
            {
                var leaderBaseUrl = ServerManager.GetNode(Request.RequestUri.Port.ToString()).NodeBase.LeaderBaseUrl;

                var client = new RestClient("http://" + leaderBaseUrl);
                var restRequest = new RestRequest("api/zoo/proxys");
                restRequest.Method = Method.GET;

                var restResponse = client.Execute(restRequest);

                var response = JsonConvert.DeserializeObject<object>(restResponse.Content);

                return response;
            }
        }

        private NodeBase GetLeaderNode()
        {
            var auth = Request.RequestUri.Authority;
            var leaderNode = ServerManager.GetLeader();

            if (leaderNode != null && leaderNode.BaseUrl == auth)
            {
                return leaderNode;
            }

            return null;
        }
    }
}