﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Xunit;

using GenHTTP.Testing.Acceptance.Domain;
using GenHTTP.Modules.Core;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Core
{

    public class CompressionTests
    {

        private class CustomAlgorithm : ICompressionAlgorithm
        {

            public string Name => "custom";

            public Priority Priority => Priority.High;

            public Stream Compress(Stream content)
            {
                return content;
            }

        }

        /// <summary>
        /// As a developer, I expect responses to be compressed out of the box.
        /// </summary>
        [Fact]
        public void TestCompression()
        {
            using var runner = TestRunner.Run();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "gzip, br");

            using var response = request.GetSafeResponse();

            Assert.Equal("gzip", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to disable compression.
        /// </summary>
        [Fact]
        public void TestCompressionDisabled()
        {
            using var runner = new TestRunner();

            using var _ = runner.Builder.Compression(false).Build();

            using var response = runner.GetResponse();

            Assert.Null(response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want to be able to add custom compression algorithms.
        /// </summary>
        [Fact]
        public void TestCustomCompression()
        {
            using var runner = new TestRunner();

            using var _ = runner.Builder.Compression(new CustomAlgorithm()).Build();

            var request = runner.GetRequest();
            request.Headers.Add("Accept-Encoding", "custom");

            using var response = request.GetSafeResponse();

            Assert.Equal("custom", response.ContentEncoding);
        }

        /// <summary>
        /// As a developer, I want already compressed content not to be compressed again.
        /// </summary>
        [Fact]
        public void TestNoAdditionalCompression()
        {
            var image = Content.From("Image!").Type(ContentType.ImageJpg);

            using var runner = TestRunner.Run(Layout.Create().Add("/uncompressed", image));

            using var response = runner.GetResponse("/uncompressed");

            Assert.Null(response.ContentEncoding);
        }

    }

}
