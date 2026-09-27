Feature: the settings window belongs to this mod

  Scenario: Mod options opens the same instance the hooks read
    When Resolve This Instead opens its settings window
    Then Resolve This Instead sees its own settings window open
    And Resolve This Instead the open window edits the same settings instance
    When I close all dialogs
    And no errors were logged
