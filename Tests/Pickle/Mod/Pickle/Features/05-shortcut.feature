Feature: the optional MainButtons shortcut

  # Hidden by its own def rather than pushed down every frame, revealable by a customization mod, and
  # opening the same settings instance as Mod options. Whether RIMMSQOL itself finds and reveals it is a
  # separate claim, played in its own pass (06-rimmsqol).

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs

  Scenario: hidden on a clean configuration, and not merely greyed out
    Then Resolve This Instead the shortcut is hidden on a clean configuration

  Scenario: revealed, it draws, enables and opens the same settings
    When Resolve This Instead reveals its shortcut as a customization mod would
    Then Resolve This Instead the shortcut is drawn and enabled
    When Resolve This Instead activates its shortcut
    Then Resolve This Instead sees its own settings window open
    And Resolve This Instead the open window edits the same settings instance
    When I close all dialogs
    And Resolve This Instead hides its shortcut again
    Then Resolve This Instead the shortcut is hidden on a clean configuration
    And no errors were logged
