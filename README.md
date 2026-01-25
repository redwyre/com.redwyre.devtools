# com.redwyre.devtools

A some tools for developers.

* Devtools window
  * Clear library - Closes unity, clears the library then restarts unity all in one click!
  * Show burst cache size
  * Clear burst cache (some files may be still in use but are moved to a "deleteme" folder)
* Experimental terminal window.
  * A command console running in a terminal window.
* Git activity import lock 
  * When a git operation is active Unity is blocked from importing.
* Unsaved changes window
  * Shows all assets that are marked as dirty and allows you to ping or attempt to save them (some assets cannot be pinged or saved from this window)


If you are using Unity 6.3 or newer
* Toolbar buttons.
  * Open C# project (Right click for an option to clean and rebuild the projects)
  * Open Project settings
  * Open project folder in explorer
  * Open terminal
  * Script recompile
  * Clear player prefs
  * Memory Leak Detection mode
  * Build Active BuildProfile (builds the active build profile to "Project/Builds/{BuildProfileName}/{productName}.exe")
  * Select Build Profile (Shows and sets the active build profile)

Due to the current implementation of Unity's toolbar customisation, you will have to explicity add any toolbar buttons you want, they can all be found under the "DevTools" menu.