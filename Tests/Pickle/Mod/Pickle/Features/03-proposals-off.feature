@requires:mlie.naturesprettysweet
Feature: A pair that only Use This Instead proposes stays off by default

  Scenario: Nature's Pretty Sweet (Continued) is loaded but its old page is not stood in for
    Then mod "mlie.naturesprettysweet" is loaded
    And no def "RTI_TestMayRequireProposed" exists
    And no def "RTI_TestNameGuardProposed" exists
