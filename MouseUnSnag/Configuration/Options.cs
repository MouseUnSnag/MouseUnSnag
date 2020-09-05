using System;

namespace MouseUnSnag.Configuration
{
    public class Options
    {
        private bool _unstick = true;
        private bool _jump = true;
        private bool _wrap = true;
        private bool _rescale = false;

        public bool Unstick
        {
            get => _unstick;
            set
            {
                if (_unstick == value)
                    return;
                _unstick = value;
                OnConfigChanged();
            }
        }

        public bool Jump
        {
            get => _jump;
            set
            {
                if (_jump == value)
                    return;
                _jump = value;
                OnConfigChanged();
            }
        }

        public bool Wrap
        {
            get => _wrap;
            set
            {
                if (_wrap == value)
                    return;
                
                _wrap = value;
                OnConfigChanged();
            }
        }

        public bool Rescale
        {
            get => _rescale;
            set
            {
                if (_rescale == value)
                    return;
                
                _rescale = value;
                OnConfigChanged();
            }
        }


        public event EventHandler ConfigChanged;
        protected virtual void OnConfigChanged()
        {
            EventHandler handler = ConfigChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

    }
}
