namespace PayrollSystemApp.Views;
 
public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();
	}

    void btnLogin_Clicked(System.Object sender, System.EventArgs e)
    { 
        VUtils.GetoPage(new LoginPage(), true); 
    }
     
    void btnRegister_Clicked(System.Object sender, System.EventArgs e)
    {
        VUtils.GetoPage(new RegisterPage(), true); 
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        Launcher.TryOpenAsync("https://programmergwin.com");
    }
}