using Godot;

public partial class MusicManager : Node
{
	public static MusicManager Instance { get; private set; }

	private const string MenuMusicPath = "res://assets/music/ShopTheme.mp3";
	private const string GameMusicPath = "res://assets/music/BulletHell.mp3";
	private const float SilentVolumeDb = -40.0f;
	private const float FullVolumeDb = -6.0f;

	[Export] public double FadeDurationSeconds = 1.0;

	private AudioStreamPlayer musicPlayer;
	private Tween fadeTween;
	private string currentMusicPath = "";
	private string requestedMusicPath = "";
	private int transitionVersion = 0;

	public override void _EnterTree()
	{
		Instance = this;
		EnsureMusicPlayer();
	}

	public override void _Ready()
	{
		EnsureMusicPlayer();
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void EnsureMusicPlayer()
	{
		if (musicPlayer != null)
		{
			return;
		}

		musicPlayer = new AudioStreamPlayer
		{
			Name = "MusicPlayer",
			VolumeDb = SilentVolumeDb
		};
		AddChild(musicPlayer);
	}

	public void PlayMenuMusic()
	{
		PlayMusic(MenuMusicPath);
	}

	public void PlayGameMusic()
	{
		PlayMusic(GameMusicPath);
	}

	private void PlayMusic(string musicPath)
	{
		if (musicPath == currentMusicPath && musicPath == requestedMusicPath && musicPlayer.Playing)
		{
			return;
		}

		requestedMusicPath = musicPath;
		transitionVersion++;

		fadeTween?.Kill();

		if (musicPlayer.Playing && currentMusicPath != "" && FadeDurationSeconds > 0.0)
		{
			int version = transitionVersion;
			fadeTween = CreateTween();
			fadeTween.TweenProperty(musicPlayer, "volume_db", SilentVolumeDb, FadeDurationSeconds * 0.5)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			fadeTween.TweenCallback(Callable.From(() => StartTrack(musicPath, version)));
			return;
		}

		StartTrack(musicPath, transitionVersion);
	}

	private void StartTrack(string musicPath, int version)
	{
		if (version != transitionVersion || musicPath != requestedMusicPath)
		{
			return;
		}

		AudioStream stream = ResourceLoader.Load<AudioStream>(musicPath);
		if (stream == null)
		{
			GD.PushWarning($"MusicManager could not load music at {musicPath}");
			return;
		}

		if (stream is AudioStreamMP3 mp3Stream)
		{
			mp3Stream.Loop = true;
		}

		currentMusicPath = musicPath;
		musicPlayer.Stream = stream;
		musicPlayer.VolumeDb = FadeDurationSeconds > 0.0 ? SilentVolumeDb : FullVolumeDb;
		musicPlayer.Play();

		if (FadeDurationSeconds <= 0.0)
		{
			return;
		}

		fadeTween = CreateTween();
		fadeTween.TweenProperty(musicPlayer, "volume_db", FullVolumeDb, FadeDurationSeconds * 0.5)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
	}
}
