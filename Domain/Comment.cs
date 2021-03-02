using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class Comment
    {
        public int Id {get;set;}
        public string Body { get; set; }
        public AppUser Author {get;set;}
        public Activity Activity { get; set; }
        public DateTime CreatedAt {get;set;} = DateTime.UtcNow;
        public Nullable<int> ParentCommentId {get;set;}
        [ForeignKey("ParentCommentId")]
        public Comment ParentComment { get; set; }

        public bool isReply { get; set; }

        public List<Comment> Replies {get;set;} = new List<Comment>();
    }
}