----------------------------- PROJECT SETUP ---------------------------
--------------
Backup/Restore the Runnymede database. Add 'websitelogin' (pwd:123qweASD), recreate 'websiteuser' (with 'websitelogin', add it to 'websiterole') .
--------------
Option 1. Add to C:\Windows\System32\drivers\etc\hosts   127.0.0.1 deva.englisharium.com
Option 2. "Alias" record using noip.com. deva.englisharium.com -> devaenglisharium.ddns.net -> 192.168.X.X
--------------
Add the website to C:\Users\Andrey\Documents\IISExpress\config\applicationhost.config manually. Visual Studio can create automatically only 'localhost' sites.
<site name="Website" id="1">
    <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="C:\Users\Andrey\Documents\Dignicom\Runnymede\Website" />
    </application>
    <bindings>
        <binding protocol="http" bindingInformation="*:80:deva.englisharium.com" />
        <binding protocol="https" bindingInformation="*:443:deva.englisharium.com" />
    </bindings>
</site>

Go to the project's Properties, Web tab, edit Project Url. Create Virtual Dirtectory. 
Edit Website.csproj as well to edit <IISUrl>??????

"netsh http add urlacl" is not needed since Visual Studio can start only "localhost" websites with IIS Express in the user mode. We will run Visual Studio as Administrator.
--------------
netsh http show sslcert > D:\Downloads\01ex01\01.txt
Generate a certificte (Pluralsight CelfCert is an option. Downside is that Chrome warns on SHA1 used, it likes SHA2).
Import the certificate into Computer (not Current User) account "Certificates (Local Computer)\Personal\Certificates"
netsh http add sslcert ipport=0.0.0.0:443 appid={00000000-0000-0000-0000-000000000000} certhash=YOURCERTHASHHERE
To satisfy browsers, import the cert into "Current User\Trusted Root Certification Authorities"
--------------
Run Visual Studio as Administrator. VS can start only "localhost" websites with IIS Express in the user mode.
--------------
-------------------------------- BOWER  ---------------------------------------
Install node.js (nodejs.org), it will install npm as well.
Install Git (git-scm.com). Read the note for Windows users on bower.io about the changing the PATH setting during Git installation. Restart cmd after installation.
Install bower (bower.io).
Install github.com/blittle/bower-installer
--------------
cd C:\Users\Andrey\Documents\Dignicom\Runnymede\Website
bower list
bower install -p -S <package>
bower update <name>
bower-installer
rem +http://bower.io
rem +http://joshbranchaud.com/blog/2014/02/19/Managing-Single-Files-With-Bower.html
rem +https://github.com/blittle/bower-installer  /* You can specify a folder and get all files inside it preserving its folder structure. */





