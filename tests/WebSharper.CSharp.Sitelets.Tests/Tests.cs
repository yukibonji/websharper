﻿using System;
using WebSharper;
using WebSharper.Web;
using WebSharper.Sitelets;
using Elt = WebSharper.Sitelets.Tests.Server.Elt;
using Attr = WebSharper.Sitelets.Tests.Server.Attr;
using Text = WebSharper.Sitelets.Tests.Server.Text;

namespace WebSharper.CSharp.Sitelets.Tests
{
    [Serializable]
    public class TestControl : Web.Control
    {
        [JavaScript]
        public override IControlBody Body =>
            new WebSharper.Sitelets.Tests.Client.Elt("div", "Hello from a web control class!");
    }

    public class SiteletTest
    {
        [JavaScript]
        public static WebSharper.Sitelets.Tests.Client.Elt SayHello()
        {
            Console.WriteLine("Hello world from System.Console!");
            JavaScript.Console.Log("Hello world from WebSharper.JavaScript.Console!");
            return new WebSharper.Sitelets.Tests.Client.Elt("div", "Hello from an inline control!");
        }

        [JavaScript]
        public static WebSharper.Sitelets.Tests.Client.Elt SayHello(string msg)
        {
            return new WebSharper.Sitelets.Tests.Client.Elt("div", $"Hello from an inline control with message: {msg}!");
        }

        [JavaScript]
        public static WebSharper.Sitelets.Tests.Client.Elt Hello =>
            new WebSharper.Sitelets.Tests.Client.Elt("div", "Hello from an inline control calling a static property!");

        public static Sitelet<object> Main =>
            new SiteletBuilder()
                .With("/", ctx =>
                    Content.Page(
                        Body:
                            Elt("div",
                                Elt("div",
                                    Elt("a", Attr("href", ctx.Link(JohnDoe)),
                                        Text("Go to John Doe's page"))),
                                Elt("form",
                                    Attr("action", ctx.Link(EmptyQueryPerson)),
                                    Elt("input", Attr("name", "first"), Attr("value", "Jane")),
                                    Elt("input", Attr("name", "last"), Attr("value", "Smith")),
                                    Elt("input", Attr("name", "age"), Attr("type", "number"), Attr("value", "42")),
                                    Elt("input", Attr("type", "submit"))),
                                new TestControl(),
                                new WebSharper.Web.InlineControl(() => SayHello()),
                                new WebSharper.Web.InlineControl(() => SayHello("ok")),
                                new WebSharper.Web.InlineControl(() => Hello)
                                )))
                .With<Person>((ctx, person) =>
                    Content.Page(
                        Body:
                            Elt("div",
                                Text(String.Format("{0} {1} is {2} years old. ",
                                    person.name.first, person.name.last, person.age)),
                                Elt("a", Attr("href", ctx.Link("/")),
                                    Text("Go back to C# sitelets tests home")))))
                .With<QueryPerson>((ctx, person) =>
                    Content.Page(
                        Body:
                            Elt("div",
                                person.age.HasValue ?
                                    Text(String.Format("{0} {1} is {2} years old. ",
                                        person.name.first, person.name.last, person.age.Value)) :
                                    Text(String.Format("{0} {1} won't tell their age.",
                                        person.name.first, person.name.last)),
                                Elt("a", Attr("href", ctx.Link("/")),
                                    Text("Go back to C# sitelets tests home")))))
                .Install();

        [EndPoint("/person")]
        public class Person
        {
            public Name name;
            public int age;
        }

        public class Name
        {
            public string first;
            public string last;
        }

        [EndPoint("/person")]
        public class QueryPerson
        {
            public QueryName name;

            [Query]
            public int? age;
        }

        public class QueryName
        {
            [Query]
            public string first;

            [Query]
            public string last;
        }

        private static Elt Elt(string name, params Web.INode[] children) => new Elt(name, children);
        private static Attr Attr(string name, string value) => new Attr(name, value);
        private static Text Text(string text) => new Text(text);
        public static Person JohnDoe => new Person { name = new Name { first = "John", last = "Doe" }, age = 30 };
        private static QueryPerson EmptyQueryPerson => new QueryPerson { name = new QueryName() };
    }
}
