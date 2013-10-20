using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MMDoCHistoryV2
{
    public partial class HelpForm : Form
    {
        public HelpForm(Form1 f)
        {
            form = f;

            InitializeComponent();
        }

        private readonly Form1 form;

        private CustomRTF rtf = null;

        private void SelectOption(int index)
        {
            if(this.curSelected == index)
                return;

            this.curSelected = index;

            Color selCol = Color.Yellow;
            button1.BackColor = index == 0 ? selCol : Color.FromKnownColor(KnownColor.Control);
            button2.BackColor = index == 2 ? selCol : Color.FromKnownColor(KnownColor.Control);
            button3.BackColor = index == 1 ? selCol : Color.FromKnownColor(KnownColor.Control);

            this.rtf.Clear(true);

            switch(index)
            {
                case 0:
                    {
                        this.rtf.AddLine("|cffff0000Warning!", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "The Forgotten Wars expansion has changed the replay file structure, in fact it broke the replay files. That means you will not be able to filter \"card played\" and \"card drawn\".", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "What is this program?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "This program will load all your replay files and display them in a convenient list that you can sort and filter in any way you like. It will also calculate stats of those replays such as your win / lose / draw per centage among others. You can also look at what deck you and your opponent played and it provides you with a log of the game in text form if you like. If you don't have any replay files this program is useless for you. :)", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "How do I use it?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "The program requires that you can load three types of files: replays, card info and localization. All of these files are in your game installation folder. Click on \"Options\" in the menu and set the correct directories if there aren't already. After you have done this you can go to \"File\" and click on \"Load\". Please be patient if you have many replay files, it may take a while to load. You can see if the program is loading in the status bar, if it's loading it will say \"Loading...\".", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Can I load someone else's replay?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Yes you can load replays from anywhere just add another directory to the options under replays that will point to the folder where this replay is in.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "How do I watch replays?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "This program is not for watching the replay, you can use your game for that. Make sure your game is not opened and open the replay file with your game exe (Game.exe) in your installation folder (as of 27.09.2013 this does not work on replay files created with Forgotten Wars expansion). This program however lets you look at the replay in text form. Right click on the replay and click \"View game log\". It can be a bit confusing because the replay file mostly only saves where someone clicked and not the monster health and status etc.", true);
                        this.rtf.AddLine("", true);

                        /*this.rtf.AddLine(CustomRTF.QuestionColor + "How does the program know what my opponent is drawing?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "The program plays through the whole game in order to get you the representation of replay, if a card was later revealed it goes back and changes all locations where this card was used to show the card true name.", true);
                        this.rtf.AddLine("", true);*/

                        /*this.rtf.AddLine(CustomRTF.QuestionColor + "Isn't that cheating?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Well, no. You don't know what the card's name is until you see the card and by then it doesn't matter what it was before because you already know what the card is anyway.", true);
                        this.rtf.AddLine("", true);*/

                        this.rtf.AddLine(CustomRTF.QuestionColor + "How come when I look at deck list of a game and some cards in opponent's deck are \"CreatureCard\" and \"SpellCard\" and \"FortuneCard\"?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "These are the cards that were never revealed during the game. It is not possible to know what the cards really are other than what type of card it is. If you are a programmer and looking for more information on this topic feel free to talk to me about it.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Who made this program?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Duckbat. You can contact me through mmdoc.net IRC chat, in game by adding player \"ytskv772\" or through the ubisoft forums. If you're low level I may decline your request in game.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "How do filters work?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Filter is a group of conditions that the replay file must pass in order to show up on the list of replays. In the filters list right click and choose \"New filter\" to create a filter. What's more important is how conditions work, check next question.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "How do conditions work?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Conditions are a function that will be applied to the replay file, if the function is not true then condition has failed, if it is true then condition has allowed replay to pass. Here are condition properties explained:", true);
                        this.rtf.AddLine("|cffffff00Type" + CustomRTF.AnswerColor + ": this is the function that will be used to check replay file. Other values of the condition depend on this function.", true);
                        this.rtf.AddLine("|cffffff00Minimum turn" + CustomRTF.AnswerColor + ": certain events in the game can be restricted to search only in specific places. For example if you choose a function where a card was played then the minimum turn value would make sure that the condition is only valid if current turn is greater or equal to this value. Game starts with turn 1, if it's other player's turn the turn will still be 1, if it's your turn again the turn will be 2 and so on. Setting this value to -1 will disable this restriction. Also only some functions will check the turn value, for example when checking player name the minimum turn value would have no importance and will be ignored.", true);
                        this.rtf.AddLine("|cffffff00Maximum turn" + CustomRTF.AnswerColor + ": same as minimum except this time turn must be this or less. Setting -1 will disable this restriction as well.", true);
                        this.rtf.AddLine("|cffffff00Minimum time" + CustomRTF.AnswerColor + ": this is the amount of time that must have passed before this condition is valid. The value is in milliseconds, if you want 38 seconds to have passed before checking this condition then set this value to 38000. Setting -1 will disable this function. Note: the timer starts when replay is created that means before the game has actually started.", true);
                        this.rtf.AddLine("|cffffff00Maximum time" + CustomRTF.AnswerColor + ": same as minimum time except other way around. Setting -1 will disable this restriction.", true);
                        this.rtf.AddLine("|cffffff00Or" + CustomRTF.AnswerColor + ": this can be complicated to explain. You can have multiple conditions in one filter. If you check this filter's Or then it is allowed for this condition to fail if the next condition in the list does not fail. You can also chain together multiple Or conditions. If Or is not checked then this condition must pass or filter will fail unless the previous condition's Or was set and it passed. Reread it a couple of times :) this is the best I can explain it.", true);
                        this.rtf.AddLine("|cffffff00Invert" + CustomRTF.AnswerColor + ": this will make the condition's check the opposite. For example if invert is checked and condition is true it will be set to false, if condition is false it will be set to true.", true);
                        this.rtf.AddLine("|cffffff00Require's my turn" + CustomRTF.AnswerColor + ": this should be easy to understand. If you use a Type like \"card was played\" a nd this is set that would mean this condition is only true if it was currently my turn when the card was played.", true);
                        this.rtf.AddLine("|cffffff00Require's opponent's turn" + CustomRTF.AnswerColor + ": same as above but opponent's turn is required.", true);
                        this.rtf.AddLine("|cffffff00Values" + CustomRTF.AnswerColor + ": these have different meaning for every type of condition. For example if you choose type \"card was played\" then there will be three values: key, value and count. Key is the card data's (XML file) attribute name. For example \"Card.Cost\" or just \"Name\" which is loaded from localization file. Value is the result, if you chose cost then card's value could be \"3\" but here you will check your own value to compare against. If you choose greater than or equal to 3 then only cards that would have cost greater than or equal to 3 would pass. And the last value is the count you want. You can set for example greater than or equal to 15 then the condition as a whole would be \"require that 15 or more cards with cost of 3 or greater were played during the game\". Simple, huh? :)", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Tips:", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Set conditions that are simple first in the filter, that would make the filtering process much faster because if a simple condition fails we don't need to check the complicated ones and can save a lot of time.", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Check all card keys in \"cards_*.xml\" files in your GameData folder.", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Filtering can be a very slow process if you have many replays or complex filters, be patient.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Some replays don't show up!", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "It is possible that a replay file can be corrupted or otherwise broken. If you want to see these replays go to \"Show\" in menu and make sure \"Hide broken replays\" is not checked.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Why don't some duels have result?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "If a result is missing that means the game is currently in progress or bugged (crash, disconnect etc.), there was no ending to the game in replay file.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Why does this program use so much memory?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "It loads all the replay files into memory. Not only the results but everything that happened in the game as well. With 6700 replays I have 360 MB memory usage, I doubt your computer has less than 2 GB RAM.", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Filters aren't working!", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "There can be multiple reasons: filter processing is paused while you have filter or condition editing window open, you made the filter wrong or it is filtering right now but it takes time. If you created complex filters or have many replays it may take a while, if filters are currently processing you can see it in the status bar, it would say \"Filtering...\".", true);
                        this.rtf.AddLine("", true);

                        this.rtf.AddLine(CustomRTF.QuestionColor + "I found a bug, crash or have ideas for improvement.", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "Feel free to contact me and I will fix (see question who made this for information).", true);
                        this.rtf.AddLine("", true);

                        /*this.rtf.AddLine(CustomRTF.QuestionColor + "Where is the source code?", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "", true);
                        this.rtf.AddLine("", true);*/
                        //TODO();

                        this.rtf.AddLine(CustomRTF.QuestionColor + "Some card names are wrong.", true);
                        this.rtf.AddLine("", true);
                        this.rtf.AddLine(CustomRTF.AnswerColor + "It is possible that the game was updated (patched) since the replay file was created. If any cards were changed or deleted then the replay parser will not be able to tell which card it is because it is using current information and the replay used different files.", true);
                        this.rtf.AddLine("", true);
                    }break;
            }

            this.rtf.Update();
        }

        private int curSelected = -1;

        private void button1_Click(object sender, EventArgs e)
        {
            this.SelectOption(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.SelectOption(1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.SelectOption(2);
        }

        private void HelpForm_Load(object sender, EventArgs e)
        {
            this.rtf = new CustomRTF(this.richTextBox1);

            this.SelectOption(0);
        }

        private void HelpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            form.ClosedForm(this);
        }
    }
}
