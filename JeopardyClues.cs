﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.312
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace Jeopardy {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2", IsNullable=false)]
    public partial class PointValues : object, System.ComponentModel.INotifyPropertyChanged {
        
        private float[] valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public float[] Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
                this.RaisePropertyChanged("Value");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2", IsNullable=false)]
    public partial class Clue : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string questionField;
        
        private string answerField;
        
        private string sourceField;
        
        /// <remarks/>
        public string Question {
            get {
                return this.questionField;
            }
            set {
                this.questionField = value;
                this.RaisePropertyChanged("Question");
            }
        }
        
        /// <remarks/>
        public string Answer {
            get {
                return this.answerField;
            }
            set {
                this.answerField = value;
                this.RaisePropertyChanged("Answer");
            }
        }
        
        /// <remarks/>
        public string Source {
            get {
                return this.sourceField;
            }
            set {
                this.sourceField = value;
                this.RaisePropertyChanged("Source");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2", IsNullable=false)]
    public partial class Category : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string nameField;
        
        private Clue[] clueField;
        
        /// <remarks/>
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
                this.RaisePropertyChanged("Name");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Clue")]
        public Clue[] Clue {
            get {
                return this.clueField;
            }
            set {
                this.clueField = value;
                this.RaisePropertyChanged("Clue");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.mvysin.com/Jeopardy/Clues/2", IsNullable=false)]
    public partial class JeopardyBoard : object, System.ComponentModel.INotifyPropertyChanged {
        
        private float[] pointValuesField;
        
        private Category[] categoryField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Value", IsNullable=false)]
        public float[] PointValues {
            get {
                return this.pointValuesField;
            }
            set {
                this.pointValuesField = value;
                this.RaisePropertyChanged("PointValues");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Category")]
        public Category[] Category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
                this.RaisePropertyChanged("Category");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
