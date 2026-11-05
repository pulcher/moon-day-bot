using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.Configuration;
using Terminal.Gui.Drawing;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

using IApplication app = Application.Create ();
app.Init ();

Scheme amberScheme = new (new Terminal.Gui.Drawing.Attribute (new Color ("#FFB000"), Color.Black));
SchemeManager.AddScheme ("Base", amberScheme);
SchemeManager.AddScheme ("Menu", amberScheme);
SchemeManager.AddScheme ("Accent", amberScheme);

List<string> transcriptLines = [];
Label transcript = new ()
{
    X = 1,
    Y = 0,
    Width = Dim.Fill (2),
    Height = Dim.Fill (1),
    Text = "No transmissions yet."
};
Label status = new ()
{
    X = 1,
    Y = 1,
    Width = Dim.Fill (2),
    Text = "UPLINK READY  |  ENCRYPTION: ACTIVE"
};
TextField input = new ()
{
    X = 1,
    Y = 3,
    Width = Dim.Fill (2),
    Text = string.Empty
};

void SubmitMessage ()
{
    string message = input.Text?.ToString ().Trim () ?? string.Empty;
    if (message.Length == 0)
    {
        status.Text = "Enter a message before transmitting.";
        return;
    }

    transcriptLines.Add ($"[{DateTime.Now:HH:mm:ss}] YOU > {message}");
    transcriptLines.Add ($"[{DateTime.Now:HH:mm:ss}] SYS > Transmission received.");
    if (transcriptLines.Count > 10)
    {
        transcriptLines.RemoveRange (0, transcriptLines.Count - 10);
    }

    transcript.Text = string.Join (Environment.NewLine, transcriptLines);
    status.Text = "TRANSMISSION SENT  |  UPLINK READY";
    input.Text = string.Empty;
}

Button transmitButton = new ()
{
    X = 1,
    Y = 5,
    Title = "_Transmit"
};
transmitButton.Accepted += (_, _) => SubmitMessage ();

Window mainWindow = new ()
{
    Title = "M A T R I X   //   TERMINAL",
    Width = Dim.Fill (),
    Height = Dim.Fill ()
};

Window transmissionWindow = new ()
{
    Title = "TRANSMISSION LOG",
    X = 0,
    Y = 1,
    Width = Dim.Percent (70),
    Height = Dim.Percent (50)
};
transmissionWindow.Add (transcript);

Window inputWindow = new ()
{
    Title = "UPLINK CONSOLE",
    X = 0,
    Y = Pos.Bottom (transmissionWindow),
    Width = Dim.Percent (70),
    Height = Dim.Fill (1)
};
inputWindow.Add (
    status,
    new Label
    {
        X = 1,
        Y = 2,
        Text = "MESSAGE"
    },
    input,
    transmitButton);

Label matrixDisplay = new ()
{
    X = 0,
    Y = 0,
    Width = Dim.Fill (),
    Height = Dim.Fill (),
};
matrixDisplay.SetScheme (amberScheme);
Window matrixWindow = new ()
{
    Title = "MATRIX FEED",
    X = Pos.Percent (70),
    Y = 1,
    Width = Dim.Fill (),
    Height = Dim.Fill (1)
};
matrixWindow.Add (matrixDisplay);

StatusBar statusBar = new (
[
    new Shortcut { Title = "F1 Menu", Key = Key.F1 },
    new Shortcut { Title = "Ctrl+Q Quit", Key = Key.Q.WithCtrl, Action = () => app.RequestStop () }
])
{
    X = 0,
    Y = Pos.AnchorEnd (),
    Width = Dim.Fill ()
};

MenuBar menuBar = new (
[
    new MenuBarItem (
        "_Session",
        [
            new MenuItem
            {
                Title = "_Clear log",
                Action = () =>
                {
                    transcriptLines.Clear ();
                    transcript.Text = "Log cleared.";
                    status.Text = "UPLINK READY  |  LOG CLEARED";
                }
            },
            new Line (),
            new MenuItem { Title = "_Quit", Action = () => app.RequestStop () }
        ]),
    new MenuBarItem (
        "_Help",
        [
            new MenuItem
            {
                Title = "_About",
                Action = () => status.Text = "MATRIX TERMINAL  |  Built with Terminal.Gui v2"
            }
        ])
])
{
    X = 0,
    Y = 0,
    Width = Dim.Fill ()
};

mainWindow.Add (menuBar, transmissionWindow, inputWindow, matrixWindow, statusBar);

MatrixRain rain = new ();
app.AddTimeout (TimeSpan.FromMilliseconds (120), () =>
{
    matrixDisplay.Text = rain.NextFrame (
        Math.Max (1, matrixDisplay.Frame.Width),
        Math.Max (1, matrixDisplay.Frame.Height));
    return true;
});

app.Run (mainWindow);

sealed class MatrixRain
{
    private readonly Random _random = new ();
    private readonly string _glyphs = "01ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int[] _heads = [];
    private int[] _trails = [];
    private int _width;
    private int _height;

    public string NextFrame (int width, int height)
    {
        if (width != _width || height != _height)
        {
            _width = width;
            _height = height;
            _heads = new int[width];
            _trails = new int[width];

            for (int column = 0; column < width; column++)
            {
                _heads[column] = _random.Next (height);
                _trails[column] = _random.Next (3, Math.Max (4, height / 2));
            }
        }

        StringBuilder frame = new (height * (width + Environment.NewLine.Length));
        for (int row = 0; row < height; row++)
        {
            for (int column = 0; column < width; column++)
            {
                int distance = (_heads[column] - row + height) % height;
                bool lit = distance < _trails[column] && _random.NextDouble () > 0.12;
                frame.Append (lit ? _glyphs[_random.Next (_glyphs.Length)] : ' ');
            }

            if (row < height - 1)
            {
                frame.AppendLine ();
            }
        }

        for (int column = 0; column < width; column++)
        {
            _heads[column] = (_heads[column] + 1) % height;
        }

        return frame.ToString ();
    }
}
