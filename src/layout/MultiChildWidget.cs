using System;
using System.Collections.Generic;
using Rendering;

namespace Layout {

public abstract class MultiChildWidget : BaseWidget {

  protected List<BaseWidget> children;

  public override IEnumerable<BaseWidget> Visit() {
    for (var i = 0; i < children.Count; i++) {
      yield return children[i];
    }
  }

}

}
