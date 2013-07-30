<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Events.aspx.cs" Inherits="WebSite.Events" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>DofusWorldEditor</title>
    <link href="./CSS/Style.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
        <div id="header"><div id="logo"></div>
        </div>

        <div id="navbar">
            <div id="menu">
                <a href="Default.aspx" class="left"><img src="./CSS/Images/accueil.png" width="36" height="36" class="icone"/>
                    <p>Accueil <br /> Nouveautés de l'éditeur</p>
                </a>
            </div>
            <div id="menu">
                <a href="Users.aspx"><img src="./CSS/Images/user.png" width="36" height="36" class="icone"/>
                    <p>Utilisateurs <br /> Utilisateurs & galeries</p>
                </a>
            </div>
            <div id="menu">
                <a href="DevBlog.aspx"><img src="./CSS/Images/blog.png" width="36" height="36" class="icone"/>
                    <p>DevBlog <br /> Développement de l'éditeur</p>
                </a>
            </div>
            <div id="menu">
                <a href="Medias.aspx"><img src="./CSS/Images/media.png" width="36" height="36" class="icone"/>
                    <p>Médias <br /> L'éditeur en images et vidéos</p>
                </a>
            </div>
            <div id="menu">
                <a href="Account.aspx"><img src="./CSS/Images/userbarbe.png" width="36" height="36" class="icone"/>
                    <p>Mon compte <br /> Gérer votre compte</p>
                </a>
            </div>
        </div>

        <div id="corps">
            <div id="menugauche">
                <h2>Site</h2>
                <ul>
                    <li style="background-image:url('./CSS/Images/icones/news.png')"><a href="Default.aspx">News</a></li>
                    <li style="background-image:url('./CSS/Images/icones/events.png')"><a href="Events.aspx">Events</a></li>
                    <li style="background-image:url('./CSS/Images/icones/FAQ.png')"><a href="FAQ.aspx">FAQ</a></li>
                </ul>
                <h2>Utilisateurs</h2>
                <ul>
                    <li style="background-image:url('./CSS/Images/icones/icon_users.png')"><a href="Users.aspx">Liste des utilisateurs</a></li>
                    <li style="background-image:url('./CSS/Images/icones/stats.png')"><a href="Stats.aspx">Statistiques des licences</a></li>
                    <li style="background-image:url('./CSS/Images/icones/bannis.png')"><a href="Banned.aspx">Utilisateur bannis</a></li>
                </ul>
                <h2>DevBlog</h2>
                <ul>
                    <li style="background-image:url('./CSS/Images/icones/news.png')"><a href="DevBlog.aspx">News</a></li>
                    <li style="background-image:url('./CSS/Images/icones/events.png')"><a href="Updates.aspx">Mises à jour</a></li>
                    <li style="background-image:url('./CSS/Images/icones/bannis.png')"><a href="Tracker.aspx">BugTracker</a></li>
                </ul>
                <h2>Médias</h2>
                <ul>
                    <li style="background-image:url('./CSS/Images/icones/news.png')"><a href="Medias.aspx">Présentation</a></li>
                    <li style="background-image:url('./CSS/Images/icones/icon_folders.png')"><a href="Videos.aspx">Vidéos</a></li>
                    <li style="background-image:url('./CSS/Images/icones/icon_pictures.png')"><a href="Pictures.aspx">Images</a></li>
                </ul>
            </div>

            <div id="corpstxt">
               <h2>Evènements</h2>
            </div>
        </div>

        <div id="footer">
            <div style="float: left;">Copyright Ghost [c] 2012 -> 2013</div><center><a href="Default.aspx">Retourner à la page d'accueil</a></center>
        </div>
    </form>
</body>
</html>
