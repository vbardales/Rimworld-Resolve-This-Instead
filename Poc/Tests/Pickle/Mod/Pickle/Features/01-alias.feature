Feature: Resolve This Instead POC: an old mod counts as loaded when its successor is

  # The POC's own C# checks (IsActive, AnyModActiveNoSuffix, name lookup) are written to Player.log as [RTI-POC] lines,
  # which the evidence folder keeps and a person reads. What Pickle can assert here is what those lookups DID to the
  # XML: whether the guarded defs exist. Pass: wsl-deps.avec-fournisseurs.map (successor, its framework, both providers;
  # the original mod is NOT staged, that is the point).

  Scenario: the original is absent and its successor is loaded
    Then mod "nelim.adaptivestorageneolithic" is loaded
    And mod "adaptive.storage.neolithic" is not loaded

  Scenario: a name guard on the original's display name applies through the alias
    Then def "RTI_PocPatchGuard" of type "StatCategoryDef" exists
    And no def "RTI_PocPatchControl" exists

  Scenario: MayRequire on the original's packageId is satisfied through the alias
    Then def "RTI_PocMayRequire" of type "StatCategoryDef" exists
    And no def "RTI_PocMayRequireControl" exists

  Scenario: XML Extensions listing both ids applies without any library
    Then def "RTI_PocXmlExtBoth" of type "StatCategoryDef" exists

  Scenario: XML Extensions on the old id alone does not apply, its method knows no replacement
    Then no def "RTI_PocXmlExtOldId" exists

  Scenario: CCL FindModByID on the old id applies through the SamePackageId postfix
    Then def "RTI_PocCclOldId" of type "StatCategoryDef" exists

  Scenario: the run raised no error
    Then no errors were logged
