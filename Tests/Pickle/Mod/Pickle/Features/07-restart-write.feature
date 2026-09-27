@requires:nelim.adaptivestorageneolithic
Feature: a settings change is written, for the next launch to read

  # First half of the restart pair, not playable on its own: it leaves the declared Neolithic alias
  # disabled on disk on purpose and stands the teardown down. Play the two together, and only together,
  # on the avec-neolithic-nps pass:
  #
  #   -DepMap wsl-deps.avec-neolithic-nps.map -Filter 07-restart-write -Then 08-restart-read
  #
  # which takes the machine lock once, stages once, and launches the game twice.

  Scenario: disabling a declared alias, and picking an ambiguous successor, are written to the settings file
    Then def "RTI_TestMayRequireDeclared" of type "StatCategoryDef" exists
    And Resolve This Instead the pair from "nelim.resolvethisinstead.test.ambiguous" is ambiguous between its loaded successors
    When Resolve This Instead disables the alias from "adaptive.storage.neolithic" to "nelim.adaptivestorageneolithic"
    And Resolve This Instead picks "Mlie.UseThisInstead" as the successor of "nelim.resolvethisinstead.test.ambiguous" for the next launch
    And Resolve This Instead keeps its settings for the next launch
    Then Resolve This Instead a settings file exists on disk
    And no errors were logged
