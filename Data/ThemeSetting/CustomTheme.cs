using MudBlazor;

namespace ClubTreasury.Data.ThemeSetting;

public static class CustomTheme
{
    public static readonly MudTheme ClubTreasuryTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#2277bb",           
            Secondary = "#C62828",         
            AppbarBackground = "#0c3f6e",  
            AppbarText = "#FFFFFF",        
            DrawerBackground = "#FFFFFF", 
            DrawerText = "#333333", 
            Background = "#F5F5F5",        
            Surface = "#FFFFFF",           
            TextPrimary = "#333333",       
            TextSecondary = "#666666",
        },
        PaletteDark = new PaletteDark()
        {
            // Darkmode in Mx Linux Adwaita dark theme
            Primary = "#3584E4",          
            Secondary = "#C62828",         
            Tertiary = "#62A0EA",          
            
            AppbarBackground = "#242424",  
            AppbarText = "#FFFFFF",        
            
            DrawerBackground = "#303030",  
            DrawerText = "#FFFFFF",        
            
            Background = "#242424",        
            BackgroundGray = "#303030",    
            Surface = "#303030",           
            
            TextPrimary = "#FFFFFF",
            TextSecondary = "#C0C0C0",
            TextDisabled = "#6E6E6E",
            
            ActionDefault = "#EBEBEB",
            ActionDisabled = "#6E6E6E",
            ActionDisabledBackground = "#3A3A3A",
            
            Divider = "#3D3D3D",
            DividerLight = "#4A4A4A",
            
            Success = "#26A269",           
            Warning = "#E5A50A",           
            Error = "#D32F2F",             
            Info = "#3584E4",              
            
            TableLines = "#3D3D3D",
            TableStriped = "#282828",
            
            HoverOpacity = 0.08,
            
            LinesDefault = "#3D3D3D",
            LinesInputs = "#5E5E5E",
        }
    };
    
}