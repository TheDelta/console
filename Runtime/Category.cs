﻿using System;
using System.Collections.Generic;

namespace Popcron.Console
{
    [Serializable]
    public class Category
    {
        private string name = "";
        private string id = "";
        private List<Command> commands = new List<Command>();

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string ID
        {
            get
            {
                return id;
            }
        }

        public bool Visible { get; set; } = true;

        public List<Command> Commands
        {
            get
            {
                return commands;
            }
        }

        private Category(string name, string id = "")
        {
            this.name = name;
            this.id = id;
        }

        public static Category CreateUncategorized()
        {
            Category category = new Category("Uncategorized");
            return category;
        }

        public static Category Create(Type type)
        {
            CategoryAttribute attribute = type.GetCategoryAttribute();
            if (attribute == null) return null;

            Category category = new Category(attribute.name, attribute.id);
            List<Command> commands = Library.Commands;
            foreach (Command command in commands)
            {
                if (command.Owner == type)
                {
                    command.Category = category;
                    category.commands.Add(command);
                }
            }

            return category;
        }
    }
}