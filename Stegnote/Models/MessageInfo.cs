using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Stegnote.Models
{
    public class MessageInfo
    {
        public MessageInfo(string text, SortedDictionary<char, int> data)
        {
            Text = text;
            MessageData = data;
        }

        public string Text { get; }
        public SortedDictionary<char, int> MessageData { get; }
    }
}
