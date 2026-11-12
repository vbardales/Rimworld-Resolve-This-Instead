Feature: the settings window's debug switch, restore button and old files

  # Played with the sans-facultatifs pass (no map). Nothing here is written to the real settings file.

  Scenario: debug logs are off by default
    Then Resolve This Instead debug logs are off on a clean configuration

  Scenario: the debug switch gates the debug lines
    When Resolve This Instead turns its debug logs on as the checkbox does
    Then Resolve This Instead a debug line is logged
    When Resolve This Instead turns its debug logs off as the checkbox does
    Then Resolve This Instead a debug line is suppressed

  Scenario: restoring defaults empties every choice
    Given Resolve This Instead has stored choices and debug logs on
    When Resolve This Instead restores its defaults as the button does
    Then Resolve This Instead no choice is stored and debug logs are off

  Scenario: a settings file from before the debug option still loads
    Then Resolve This Instead reads a settings file written before the debug option existed
    And no errors were logged
