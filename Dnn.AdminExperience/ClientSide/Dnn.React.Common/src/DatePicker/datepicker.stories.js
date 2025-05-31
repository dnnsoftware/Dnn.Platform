import React from "react";
import { action } from "@storybook/addon-actions";
import DatePicker from "./index";
import Label from "../Label";

let startDate = new Date("December 17, 2018 03:24:00");
export const WithContent = () => (
  <div className="scheduler-date-row">
    <Label label="Start Date" />
    <DatePicker
      date={startDate}
      updateDate={date => action("changed " + date.toString())}
      isDateRange={false}
      hasTimePicker={true}
      showClearDateButton={false}
    />
  </div>
);
