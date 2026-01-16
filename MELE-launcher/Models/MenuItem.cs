using System;

namespace MassEffectLauncher.Models
{
    public class MenuItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Action OnSelect { get; set; }
        public bool IsEnabled { get; set; }
        public MenuItemType Type { get; set; }
        public object Tag { get; set; }

        public MenuItem()
        {
            IsEnabled = true;
        }
    }
}
