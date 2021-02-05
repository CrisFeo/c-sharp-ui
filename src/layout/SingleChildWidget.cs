using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public abstract class SingleChildWidget : BaseWidget {

  protected BaseWidget child;

  public override IEnumerable<BaseWidget> Visit() {
    if (child == null) yield break;
    yield return child;
  }

}

}
