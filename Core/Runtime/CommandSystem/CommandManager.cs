using System.Collections.Generic;

public class CommandManager
{
    private Stack<ICommand> undoStack = new Stack<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        // When a new command is executed, any redo history becomes invalid.
        redoStack.Clear();
        command.Execute();
        undoStack.Push(command);
    }

    public void UndoCommand()
    {
        if (undoStack.Count > 0)
        {
            ICommand commandToUndo = undoStack.Pop();
            commandToUndo.Undo();
            redoStack.Push(commandToUndo);
        }
    }

    public void RedoCommand()
    {
        if (redoStack.Count > 0)
        {
            ICommand commandToRedo = redoStack.Pop();
            commandToRedo.Execute();
            undoStack.Push(commandToRedo);
        }
    }

    public void ClearCommandHistory()
    {
        undoStack.Clear();
        redoStack.Clear();
    }

    public int GetUndoCommandsCount()
    {
        return undoStack.Count;
    }

    public int GetRedoCommandsCount()
    {
        return redoStack.Count;
    }
}