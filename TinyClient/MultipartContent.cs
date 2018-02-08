﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TinyClient.Helpers;
using TinyClient.Response;

namespace TinyClient
{
    public class MultipartContent : IContent
    {
        private readonly HttpClientRequest[] _subRequests;
        private readonly string _boundary;

        public MultipartContent(HttpClientRequest[] subRequests, string boundary)
        {
            _subRequests = subRequests;
            _boundary = boundary;
        }

        public string ContentType => HttpMediaTypes.Mixed;
        public byte[] GetDataFor(Uri host)
        {
            var stream = new MemoryStream();
            var sb = new StringBuilder();
            var deserializers = new List<IResponseDeserializer>(_subRequests.Length);
            foreach (var request in _subRequests)
            {

                sb.AppendLine(BatchParseHelper.GetOpenBoundaryString(_boundary));
                sb.AppendLine(HttpHelper.HttpRequestContentTypeHeaderString);
                sb.AppendLine();
                sb.AppendLine($"{request.Method.Name} {request.Query.AbsoluteUri} {HttpHelper.Http11VersionCaption}");
                sb.AppendLine($"Host: {host.Host}");
                sb.AppendLine();

                stream.Write(Encoding.UTF8.GetBytes(sb.ToString()));

                stream.Write(request.Content.GetDataFor(host));
                deserializers.Add(request.Deserializer);
            }
            stream.Write(Encoding.UTF8.GetBytes("\r\n--" + _boundary));
            return stream.ToArray();
        }
    }
}