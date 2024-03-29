﻿using ActionCalculator.Models;
using Microsoft.AspNetCore.Components;
using Block = ActionCalculator.Models.Actions.Block;
using Player = ActionCalculator.Models.Player;
using Dodge = ActionCalculator.Models.Actions.Dodge;
using Action = ActionCalculator.Models.Actions.Action;

namespace BloodBowlDave.Client.Pages.Components.Calculation
{
    public partial class ActionSummary
    {
        private int Successes { get; set; }

        [Parameter]
        public PlayerAction PlayerAction { get; set; } = null!;

        [Parameter]
        public int Index { get; set; }
        
        [Parameter]
        public EventCallback<int> RemoveAction { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnToggleBreakTackle { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnToggleDivingTackle { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnToggleBrawler { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnTogglePro { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnToggleRerollInaccurate { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, bool>> OnToggleRerollFailure { get; set; }

        [Parameter]
        public EventCallback<Tuple<int, int>> OnSuccessesChanged { get; set; }

        [Parameter]
        public ActionType? LastActionType { get; set; }

        [Parameter]
        public ActionType? PenultimateActionType { get; set; }

        private Player Player => PlayerAction.Player;
        private Action Action => PlayerAction.Action;

        private void RemoveActionByIndex(int i)
        {
            RemoveAction.InvokeAsync(i);
        }

        private bool RerollFailure() => ((ActionCalculator.Models.Actions.Dauntless)Action).RerollFailure;

        private bool BreakTackle() => ((Dodge)Action).UseBreakTackle;

        private bool DivingTackle() => ((Dodge)Action).UseDivingTackle;

        private bool UseBrawler() => ((Block)Action).UseBrawler;

        private bool UsePro() => Action.UsePro;

        private bool RerollInaccurate() => Action.ActionType switch
        {
            ActionType.Pass => ((ActionCalculator.Models.Actions.Pass)Action).RerollInaccuratePass,
            ActionType.ThrowTeammate => ((ActionCalculator.Models.Actions.ThrowTeammate)Action).RerollInaccurateThrow,
            _ => false
        };

        private void ToggleBreakTackle()
        {
            OnToggleBreakTackle.InvokeAsync(new Tuple<int, bool>(Index, !BreakTackle()));
        }

        private void ToggleDivingTackle()
        {
            OnToggleDivingTackle.InvokeAsync(new Tuple<int, bool>(Index, !DivingTackle()));
        }

        private void ToggleBrawler()
        {
            OnToggleBrawler.InvokeAsync(new Tuple<int, bool>(Index, !UseBrawler()));
        }

        private void TogglePro()
        {
            OnTogglePro.InvokeAsync(new Tuple<int, bool>(Index, !UsePro()));
        }

        private void ToggleRerollInaccurate()
        {
            OnToggleRerollInaccurate.InvokeAsync(new Tuple<int, bool>(Index, !RerollInaccurate()));
        }

        private void ToggleRerollFailure()
        {
            OnToggleRerollFailure.InvokeAsync(new Tuple<int, bool>(Index, !RerollFailure()));
        }

        private void SuccessesChanged(int successes)
        {
            OnSuccessesChanged.InvokeAsync(new Tuple<int, int>(Index, successes));
        }
    }
}

