using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class CommonSkillItem : UIBase
    {
        private int _id;
        private GButton _icon;
        private GTextField _name;
        private GTextField _desc;

        public CommonSkillItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GButton>("icon");
            _name = GetGObjectChild<GTextField>("name");
            _desc = GetGObjectChild<GTextField>("desc");

            _icon.onClick.Add(OnClickIcon);
        }

        public void Update(int id)
        {
            this._id = id;
            var config = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", id);
            _icon.icon = config.Icon;
            _name.text = config.Name;
            _desc.text = config.Desc;
        }

        private void OnClickIcon(EventContext context)
        {
            //var pos = context.inputEvent.position;
            ////pos.y = Screen.height - pos.y;
            //pos = GRoot.inst.GlobalToLocal(pos);

            //var config = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _id);
            //var attrs = new List<AttrStruct>();
            //foreach (var v in config.att)
            //{
            //    attrs.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name, v.value.ToString()));
            //}
            //EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Show_Attrs, new object[] {pos});
        }
    }
}
