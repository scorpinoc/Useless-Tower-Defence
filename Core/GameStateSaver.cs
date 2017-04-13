﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Core
{
    public sealed class GameStateSaver
    {
        public void SaveTo(string file, IEnumerable<GameState> states)
        {
            new XElement("GameStateSaves",
                states.Select(
                    state =>
                        new XElement(nameof(GameState),
                            new List<object>
                            {
                                new XElement(nameof(state.Cells),
                                    state.Cells.Select(
                                        cell =>
                                        {
                                            switch (cell.GetType().Name)
                                            {
                                                case nameof(RoadCell):
                                                    return new XElement(nameof(RoadCell));
                                                case nameof(TowerCell):
                                                    return new XElement(nameof(TowerCell),
                                                        new XAttribute(nameof(ITower), ((TowerCell) cell).Tower?.TowerType ?? TowerType.Empty));
                                                default:
                                                    return null;
                                            }
                                        })),
                                new XElement(nameof(GameState.Size),
                                    new List<XAttribute>
                                    {
                                        new XAttribute(nameof(GameState.Size.Height), state.GridSize.Height),
                                        new XAttribute(nameof(GameState.Size.Width), state.GridSize.Width)
                                    }),
                                new XAttribute(nameof(GameState.Lives), state.Lives),
                                new XAttribute(nameof(GameState.Gold), state.Gold),
                                new XAttribute(nameof(GameState.Level), state.Level),
                                new XAttribute(nameof(GameState.CurrentTurn), state.CurrentTurn),
                                new XAttribute(nameof(GameState.EnemiesLeft), state.EnemiesLeft)
                            }))).Save(file);
        }

        public IEnumerable<GameState> LoadFrom(string file)
        {
            Func<XElement, string, int> getInt = (element, s) => int.Parse(element.Attribute(s)?.Value ?? "0");

            var a = new TowerFactory();

            return XElement.Load(file).Elements().Select(element =>
            {
                var gameCells =
                    element.Element(nameof(GameState.Cells))?
                        .Elements()
                        .Select(new Func<XElement, GameCell>(
                            xElement =>
                            {
                                switch (xElement.Name.ToString())
                                {
                                    case nameof(RoadCell):
                                        return new RoadCell();
                                    case nameof(TowerCell):
                                        return
                                            new TowerCell(
                                                a.GetTower(
                                                    (TowerType)
                                                        Enum.Parse(typeof (TowerType),
                                                            xElement.Attribute(nameof(ITower))?.Value ?? string.Empty)));
                                    default:
                                        return null;
                                }
                            }));
                
                var gridSizeElement = element.Element(nameof(GameState.Size));
                var size = new GameState.Size
                {
                    Width = getInt(gridSizeElement, nameof(GameState.Size.Width)),
                    Height = getInt(gridSizeElement, nameof(GameState.Size.Height))
                };

                var gameState =
                    new GameState(
                        new ObservableCollection<GameCell>(gameCells ?? new[] { new RoadCell() }), size)
                        .SetLivesTo(getInt(element, nameof(GameState.Lives)))
                        .SetGoldTo(getInt(element, nameof(GameState.Gold)))
                        .SetLevelTo(getInt(element, nameof(GameState.Level)))
                        .SetCurrentTurnTo(getInt(element, nameof(GameState.CurrentTurn)))
                        .SetEnemiesTo(getInt(element, nameof(GameState.EnemiesLeft)));
                return gameState;
            });
        }
    }
}