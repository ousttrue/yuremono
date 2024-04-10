// ------------------------------------------------------------------------------------------------
// 入力
// ------------------------------------------------------------------------------------------------


// -------------------------------------------------------------------------------------------
// ボタンの状態（ビット）
const yrInputButtonStatusBit =
{
  just: 1,
  pushing: 2,
  release: 4
}

// -------------------------------------------------------------------------------------------
// 入力
export class yrInput {
  _mouse_pos_x = 0;
  _mouse_pos_y = 0;
  _mouse_npos_x = 0.0;
  _mouse_npos_y = 0.0;
  _mouse_move_x = 0;
  _mouse_move_y = 0;
  _mouse_nmove_x = 0.0;
  _mouse_nmove_y = 0.0;
  _mouse_button_l = false;
  _mouse_button_m = false;
  _mouse_button_r = false;
  _mouse_button_status_l = 0;
  _mouse_button_status_m = 0;
  _mouse_button_status_r = 0;
  _key_button_w = false;		// 動的な連想配列か何かにしたい
  _key_button_s = false;		// 
  _key_button_a = false;		// 
  _key_button_d = false;		// 
  _key_button_status_w = 0;	// 
  _key_button_status_s = 0;	// 
  _key_button_status_a = 0;	// 
  _key_button_status_d = 0;	// 

  // -------------------------------------------------------------------------------------------
  // 一時記憶用
  __mouse_pos_x_temp__ = 0;
  __mouse_pos_y_temp__ = 0;
  __mouse_npos_x_temp__ = 0.0;
  __mouse_npos_y_temp__ = 0.0;
  __mouse_button_l_temp__ = false;
  __mouse_button_m_temp__ = false;
  __mouse_button_r_temp__ = false;
  __key_button_w_temp__ = false;	// 動的な連想配列か何かにしたい
  __key_button_s_temp__ = false;	// 
  __key_button_a_temp__ = false;	// 
  __key_button_d_temp__ = false;	// 
  __touch_id__ = -1;

  // -------------------------------------------------------------------------------------------
  // マウスイベント
  __onMouseEvent__(e: MouseEvent) {
    this.__mouse_pos_x_temp__ = e.offsetX;
    this.__mouse_pos_y_temp__ = e.offsetY;
    this.__mouse_npos_x_temp__ = this.__mouse_pos_x_temp__ / (e.target as HTMLElement).offsetWidth * 2.0 - 1.0;
    this.__mouse_npos_y_temp__ = -(this.__mouse_pos_y_temp__ / (e.target as HTMLElement).offsetHeight * 2.0 - 1.0);
    this.__mouse_button_l_temp__ = (0 != (e.buttons & 1));
    this.__mouse_button_m_temp__ = (0 != (e.buttons & 4));
    this.__mouse_button_r_temp__ = (0 != (e.buttons & 2));
  }

  // -------------------------------------------------------------------------------------------
  // タッチイベント
  __onTouchStartEvent__(e: TouchEvent) {
    e.preventDefault();

    if (-1 == this.__touch_id__) {
      this.__touch_id__ = e.targetTouches[0].identifier;

      this.__mouse_pos_x_temp__ = e.targetTouches[0].pageX - (e.target as HTMLElement).offsetLeft;
      this.__mouse_pos_y_temp__ = e.targetTouches[0].pageY - (e.target as HTMLElement).offsetTop;
      this.__mouse_npos_x_temp__ = this.__mouse_pos_x_temp__ / (e.target as HTMLElement).offsetWidth * 2.0 - 1.0;
      this.__mouse_npos_y_temp__ = -(this.__mouse_pos_y_temp__ / (e.target as HTMLElement).offsetHeight * 2.0 - 1.0);

      this.__mouse_button_l_temp__ = true;
    }
  }

  __onTouchEndEvent__(e: TouchEvent) {
    //	e.preventDefault();

    for (var i in e.changedTouches) {
      if (this.__touch_id__ == e.changedTouches[i].identifier) {
        this.__touch_id__ = -1;

        this.__mouse_pos_x_temp__ = e.changedTouches[i].pageX - (e.target as HTMLElement).offsetLeft;
        this.__mouse_pos_y_temp__ = e.changedTouches[i].pageY - (e.target as HTMLElement).offsetTop;
        this.__mouse_npos_x_temp__ = this.__mouse_pos_x_temp__ / (e.target as HTMLElement).offsetWidth * 2.0 - 1.0;
        this.__mouse_npos_y_temp__ = -(this.__mouse_pos_y_temp__ / (e.target as HTMLElement).offsetHeight * 2.0 - 1.0);

        this.__mouse_button_l_temp__ = false;
      }
    }
  }

  __onTouchMoveEvent__(e: TouchEvent) {
    //	e.preventDefault();

    for (var i in e.changedTouches) {
      if (this.__touch_id__ == e.changedTouches[i].identifier) {
        this.__mouse_pos_x_temp__ = e.changedTouches[i].pageX - (e.target as HTMLElement).offsetLeft;
        this.__mouse_pos_y_temp__ = e.changedTouches[i].pageY - (e.target as HTMLElement).offsetTop;
        this.__mouse_npos_x_temp__ = this.__mouse_pos_x_temp__ / (e.target as HTMLElement).offsetWidth * 2.0 - 1.0;
        this.__mouse_npos_y_temp__ = -(this.__mouse_pos_y_temp__ / (e.target as HTMLElement).offsetHeight * 2.0 - 1.0);
      }
    }
  }

  __onTouchCancelEvent__(e: TouchEvent) {
    this.__touch_id__ = -1;
    this.__mouse_button_l_temp__ = false;
  }

  // -------------------------------------------------------------------------------------------
  // キーボードイベント
  __onKeyDown__(e: KeyboardEvent) {
    switch (e.keyCode) {
      case 87:
        this.__key_button_w_temp__ = true;
        break;
      case 83:
        this.__key_button_s_temp__ = true;
        break;
      case 65:
        this.__key_button_a_temp__ = true;
        break;
      case 68:
        this.__key_button_d_temp__ = true;
        break;
    }
  }

  __onKeyUp__(e: KeyboardEvent) {
    switch (e.keyCode) {
      case 87:
        this.__key_button_w_temp__ = false;
        break;
      case 83:
        this.__key_button_s_temp__ = false;
        break;
      case 65:
        this.__key_button_a_temp__ = false;
        break;
      case 68:
        this.__key_button_d_temp__ = false;
        break;
    }
  }

  constructor(target: HTMLElement) {
    // イベントリスナー追加
    target.addEventListener("mousedown", e => this.__onMouseEvent__(e));			// マウスイベント
    target.addEventListener("mouseup", e => this.__onMouseEvent__(e));			// 
    target.addEventListener("mouseover", e => this.__onMouseEvent__(e));			// 
    target.addEventListener("mouseout", e => this.__onMouseEvent__(e));			// 
    target.addEventListener("mousemove", e => this.__onMouseEvent__(e));			// 
    target.addEventListener("touchstart", e => this.__onTouchStartEvent__(e));	// タッチイベント
    target.addEventListener("touchend", e => this.__onTouchEndEvent__(e));		// 
    target.addEventListener("touchmove", e => this.__onTouchMoveEvent__(e));		// 
    target.addEventListener("touchcancel", e => this.__onTouchCancelEvent__(e));	// 
    document.addEventListener("keydown", e => this.__onKeyDown__(e));			// キーボードイベント
    document.addEventListener("keyup", e => this.__onKeyUp__(e));				// 
  }

  // 更新
  update() {
    this._mouse_move_x = this.__mouse_pos_x_temp__ - this._mouse_pos_x;
    this._mouse_move_y = this.__mouse_pos_y_temp__ - this._mouse_pos_y;
    this._mouse_nmove_x = this.__mouse_npos_x_temp__ - this._mouse_npos_x;
    this._mouse_nmove_y = this.__mouse_npos_y_temp__ - this._mouse_npos_y;
    this._mouse_pos_x = this.__mouse_pos_x_temp__;
    this._mouse_pos_y = this.__mouse_pos_y_temp__;
    this._mouse_npos_x = this.__mouse_npos_x_temp__;
    this._mouse_npos_y = this.__mouse_npos_y_temp__;
    this._mouse_button_status_l = this.checkButtonStatus(this._mouse_button_l, this.__mouse_button_l_temp__);
    this._mouse_button_status_m = this.checkButtonStatus(this._mouse_button_m, this.__mouse_button_m_temp__);
    this._mouse_button_status_r = this.checkButtonStatus(this._mouse_button_r, this.__mouse_button_r_temp__);
    this._mouse_button_l = this.__mouse_button_l_temp__;
    this._mouse_button_m = this.__mouse_button_m_temp__;
    this._mouse_button_r = this.__mouse_button_r_temp__;
    this._key_button_status_w = this.checkButtonStatus(this._key_button_w, this.__key_button_w_temp__);
    this._key_button_status_s = this.checkButtonStatus(this._key_button_s, this.__key_button_s_temp__);
    this._key_button_status_a = this.checkButtonStatus(this._key_button_a, this.__key_button_a_temp__);
    this._key_button_status_d = this.checkButtonStatus(this._key_button_d, this.__key_button_d_temp__);
    this._key_button_w = this.__key_button_w_temp__;
    this._key_button_s = this.__key_button_s_temp__;
    this._key_button_a = this.__key_button_a_temp__;
    this._key_button_d = this.__key_button_d_temp__;
    // this.__mouse_move_x_temp__ = 0;
    // this.__mouse_move_y_temp__ = 0;
  }

  // ボタンの状態を取得する
  checkButtonStatus(old, now) {
    var out = 0;

    if (!now) {
      if (old) {
        out |= yrInputButtonStatusBit.release;
      }
    }
    else {
      if (!old) {
        out |= yrInputButtonStatusBit.just;
      }
      out |= yrInputButtonStatusBit.pushing;
    }

    return out;
  }

  // デバッグ表示
  debug(debug: string) {
    if (document.getElementById(debug)) {
      document.getElementById(debug).innerHTML += "mouse pos : " + this._mouse_pos_x + " " + this._mouse_pos_y + "<br>";
      document.getElementById(debug).innerHTML += "mouse npos : " + this._mouse_npos_x + " " + this._mouse_npos_y + "<br>";
      document.getElementById(debug).innerHTML += "mouse move : " + this._mouse_move_x + " " + this._mouse_move_y + "<br>";
      document.getElementById(debug).innerHTML += "mouse nmove : " + this._mouse_nmove_x + " " + this._mouse_nmove_y + "<br>";
      document.getElementById(debug).innerHTML += "mouse button : " + this._mouse_button_l + " " + this._mouse_button_m + " " + this._mouse_button_r + "<br>";
      document.getElementById(debug).innerHTML += "mouse button status : " + this._mouse_button_status_l + " " + this._mouse_button_status_m + " " + this._mouse_button_status_r + "<br>";
      document.getElementById(debug).innerHTML += "key button : " + this._key_button_w + " " + this._key_button_s + " " + this._key_button_a + " " + this._key_button_d + "<br>";
      document.getElementById(debug).innerHTML += "key button status : " + this._key_button_status_w + " " + this._key_button_status_s + " " + this._key_button_status_a + " " + this._key_button_status_d + "<br>";
    }
  }
}

