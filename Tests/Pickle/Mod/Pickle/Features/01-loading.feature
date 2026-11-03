Feature: Resolve This Instead loads

  # Every pass: the library and its hard dependencies start with no error, and the optional shortcut exists but is not shown.

  Scenario: the library loads after its dependencies without startup errors
    Then mod "nelim.resolvethisinstead" is loaded
    And mod "nelim.resolvethisinstead" loads after "brrainz.harmony"
    And mod "nelim.resolvethisinstead" loads after "mlie.usethisinstead"
    And no errors were logged

  Scenario: the settings shortcut exists
    Then def "RTI_Settings" of type "MainButtonDef" exists

  Scenario: aliases answer only while the game loads its data
    Then Resolve This Instead: the load is over and aliases no longer answer

  Scenario: Use This Instead's data was found
    Then Resolve This Instead found Use This Instead's data
