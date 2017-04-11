using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Core
{
    public class GameStateSaver
    {
        public void SaveTo(string file, IEnumerable<GameState> states)
            => new XElement("GameStateSaves",
                states.Select(
                    state =>
                        new XElement(nameof(GameState),
                            new List<object>
                            {
                                new XElement(nameof(state.Cells),
                                    state.Cells.Select(
                                        cell =>
                                            new XElement(nameof(GameCell),
                                                new XAttribute(nameof(GameCell.State), cell.State)))),
                                new XElement(nameof(state.GridSize),
                                    new List<XAttribute>
                                    {
                                        new XAttribute(nameof(state.GridSize.Height), state.GridSize.Height),
                                        new XAttribute(nameof(state.GridSize.Width), state.GridSize.Width)
                                    }),
                                new XAttribute(nameof(GameState.Lives), state.Lives),
                                new XAttribute(nameof(GameState.Gold), state.Gold),
                                new XAttribute(nameof(GameState.Level), state.Level),
                                new XAttribute(nameof(GameState.CurrentTurn), state.CurrentTurn),
                                new XAttribute(nameof(GameState.EnemiesLeft), state.EnemiesLeft)
                            }))).Save(file);
        // // old code
        //  new XElement("GameStateSaves",
        //    states.Select(
        //        state =>
        //            new XElement(nameof(GameState),
        //                typeof (GameState).GetProperties()
        //                    .Select(info => new XAttribute(info.Name, info.GetValue(state)))))).Save(file);

        public IEnumerable<GameState> LoadFrom(string file)
        {
            Func<XElement, string, int> getInt = (element, s) => int.Parse(element.Attribute(s)?.Value ?? "0");

            return XElement.Load(file).Elements().Select(element =>
            {
                var gameCells =
                    element.Element((nameof(GameState.Cells)))?
                        .Elements()
                        .Select(
                            xElement =>
                                new GameCell(
                                    (GameCell.CellState)
                                        Enum.Parse(typeof(GameCell.CellState),
                                            xElement.Attribute(nameof(GameCell.State))?.Value ??
                                            GameCell.CellState.Empty.ToString())));
                var gridSizeElement = element.Element(nameof(GameState.GridSize));
                var size = new GameState.Size
                {
                    Width = getInt(gridSizeElement, nameof(GameState.Size.Width)),
                    Height = getInt(gridSizeElement, nameof(GameState.Size.Height))
                };

                var gameState =
                    new GameState(
                        new ObservableCollection<GameCell>(gameCells ?? new[] { new GameCell(GameCell.CellState.Empty) }), size)
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