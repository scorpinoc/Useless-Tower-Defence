using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Core.GameCells;

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
                            new XElement(nameof(state.Cells), state.Cells.Select(cell =>
                            {
                                switch (cell.GetType().Name)
                                {
                                    case nameof(RoadCell):
                                        return new XElement(nameof(RoadCell));
                                    case nameof(TowerCell):
                                        {
                                            var tower = (cell as TowerCell)?.Tower;
                                            return new XElement(nameof(TowerCell),
                                                new XElement(nameof(ITower),
                                                    new XAttribute(nameof(ITower.Name), tower?.Name ?? string.Empty),
                                                    new XAttribute(nameof(ITower.AttackPower), tower?.AttackPower ?? 0),
                                                     new XAttribute(nameof(ITower.AttackSpeed), tower?.AttackSpeed ?? TimeSpan.MinValue)));
                                        }
                                    default:
                                        return null;
                                }
                            })),
                            new XElement(nameof(GameState.Size),
                                new XAttribute(nameof(GameState.Size.Height), state.GridSize.Height),
                                new XAttribute(nameof(GameState.Size.Width), state.GridSize.Width)),
                            new XAttribute(nameof(GameState.Lives), state.Lives),
                            new XAttribute(nameof(GameState.Gold), state.Gold),
                            new XAttribute(nameof(GameState.Level), state.Level),
                            new XAttribute(nameof(GameState.CurrentTurn), state.CurrentTurn),
                            new XAttribute(nameof(GameState.EnemiesLeft), state.EnemiesLeft),
                            new XAttribute(nameof(GameState.Score), state.Score)))).Save(file);
        }

        public IEnumerable<GameState> LoadFrom(string file)
        {
            Func<XElement, string, int> getInt = (element, s) => int.Parse(element.Attribute(s)?.Value ?? "0");

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
                                        {
                                            var iTower = xElement.Nodes().OfType<XElement>().First(node => node.Name == nameof(ITower));
                                            return
                                                new TowerCell(new Tower(
                                                    iTower.Attribute(nameof(ITower.Name))?.Value,
                                                    Convert.ToInt32(iTower.Attribute(nameof(ITower.AttackPower))?.Value),
                                                    TimeSpan.Parse(iTower.Attribute(nameof(ITower.AttackSpeed))?.Value)));
                                        }
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
                        .SetScoreTo(getInt(element, nameof(GameState.Score)))
                        .SetGoldTo(getInt(element, nameof(GameState.Gold)))
                        .SetLevelTo(getInt(element, nameof(GameState.Level)))
                        .SetCurrentTurnTo(getInt(element, nameof(GameState.CurrentTurn)))
                        .SetEnemiesTo(getInt(element, nameof(GameState.EnemiesLeft)));
                return gameState;
            });
        }
    }
}