using System;
using System.Collections.Generic;

namespace FluentNHibernate.Specs.FluentInterface.Fixtures
{
    class Blog : EntityBase
    {
        public IDictionary<string, string> UrlAliases { get; set; }
        public IDictionary<string, Post> Permalinks { get; set; }
        public IDictionary<Post, User> Commentors { get; set; }
    }

    class User : EntityBase
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class Post : EntityBase
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}