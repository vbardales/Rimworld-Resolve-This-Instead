Feature: several successors of the same old id at the same distance are ambiguous

  # Synthetic on purpose (see About/ResolveThisInstead.xml): the real case, Lord of the Rims - Elves, has only one of its two "Continued"
  # pages installed on this machine, so this uses a fictitious old id against two of our own always-staged hard dependencies instead.
  # No @requires: both branches are always available, in every pass.

  Scenario: an ambiguous pair resolves to neither successor on its own
    Then Resolve This Instead the pair from "nelim.resolvethisinstead.test.ambiguous" is ambiguous between its loaded successors

  Scenario: the same ambiguity through the real MayRequire door: neither successor's content applies
    Then no def "RTI_TestMayRequireAmbiguous" exists

  Scenario: no error came from the ambiguous pair
    Then no errors were logged
