@requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol @rimmsqol
Feature: RIMMSQOL finds, reveals and hides the shortcut

  # Needs the pass that stages RIMMSQOL (wsl-deps.avec-rimmsqol.map); skipped in the other passes, and a
  # skip is not a pass. Reveals through RIMMSQOL's own settings calls, not pixel clicks.

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: RIMMSQOL lists the button, hidden by default and not drawn
    Then RIMMSQOL is ready to be driven
    And RIMMSQOL's own list of main buttons offers "RTI_Settings"
    And RIMMSQOL shows the main button "RTI_Settings" as hidden
    And the main bar does not draw the button "RTI_Settings"

  Scenario: revealed through RIMMSQOL it is drawn, enabled and opens the same settings
    When RIMMSQOL reveals the main button "RTI_Settings"
    Then the main bar draws the button "RTI_Settings"
    When the main bar's button "RTI_Settings" is activated
    Then Resolve This Instead sees its own settings window open
    And Resolve This Instead the open window edits the same settings instance
    When I close all dialogs
    And RIMMSQOL hides the main button "RTI_Settings"
    Then the main bar does not draw the button "RTI_Settings"
    And no errors were logged
