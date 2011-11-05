using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Vysin.QuizShow
{
    public enum NotifyAction
    {
        Invalid = 0,
        ButtonClicked,
        DisplayBoard,
        DisplayClue,
        DisplayAnswer,
        TrackButtonChanged
    }

    public interface IBoardView
    {
        void Notify(NotifyAction a, int x, int y);
    }

    public partial class Category
    {
        public Category()
        {
            nameField = String.Empty;
            clueField = new Clue[5];
            for (int i = 0; i < 5; i++)
                clueField[i] = new Clue();
        }

        public void AddClue(Clue c)
        {
            Clue[] clues = new Clue[clueField.Length+1];
            clueField.CopyTo(clues, 0);
            clues[clues.Length-1] = c;
            clueField = clues;
        }
    }

    public partial class Clue
    {
        public Clue()
        {
            questionField = answerField = String.Empty;
        }
    }

    public partial class Board
    {
        List<WeakReference> views = new List<WeakReference>();

        public Board()
        {
            categoryField = new Category[6];
            for (int i = 0; i < 6; i++)
                categoryField[i] = new Category();
            pointValuesField = new float[] { 100F, 200F, 300F, 400F, 500F };
        }

        public void AddCategory(Category c)
        {
            Category[] cats = new Category[categoryField.Length+1];
            categoryField.CopyTo(cats, 0);
            cats[cats.Length-1] = c;
            categoryField = cats;
        }

        public void AddView(IBoardView v)
        {
            views.Add(new WeakReference(v));
        }

        public void NotifyViews(NotifyAction a, int x, int y)
        {
            foreach (WeakReference v in views)
            {
                if (v.IsAlive)
                    ((IBoardView)v.Target).Notify(a, x, y);
            }
        }
    }
}